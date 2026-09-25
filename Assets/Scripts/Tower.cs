using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Targeting")]
    public float range = 6f;
    public float fireRate = 1f; // shots per second
    public int damage = 15;
    [Tooltip("Each shot is a volley at this many of the nearest monsters in range (each hit " +
             "deals full damage). Towers and the castle use 2; towns use 1. Max 4.")]
    [Range(1, MaxTargets)] public int targetsPerShot = 1;

    [Header("Projectile")]
    [Tooltip("Placeholder shot prefab (a small sphere for now; swap for an arrow later). " +
             "If empty, the tower falls back to instant-hit damage.")]
    public GameObject projectilePrefab;
    [Tooltip("Where shots spawn from. If empty, they spawn at the tower's position.")]
    public Transform firePoint;

    [Header("Kind")]
    [Tooltip("A town is a building beside the road: it shoots weakly, Attack Towers targets it " +
             "like a tower, but damaging it pays no plunder and destroying it pays no bounty or " +
             "cap. GameManager starts a finite tribute drip instead (GDD §3 \"Encounter economy\").")]
    public bool isTown;

    [Header("Health (destructible)")]
    public int maxHealth = 100;
    [Tooltip("Flat damage cut from every hit (min 1 per hit). Towers are armored; towns are not.")]
    public int armor;
    private int health;
    public bool IsDestroyed { get; private set; }

    [Header("Alarm repair (GDD §3 \"Alarm\")")]
    [Tooltip("HP regained per second for each alarm level, once the tower hasn't been hit for " +
             "Repair Delay seconds. Punishes hit-and-run and slow, split attacks.")]
    public float repairPerAlarmLevel = 4f;
    public float repairDelaySeconds = 4f;
    private float lastHitTime = -999f;
    private float repairCarry;

    [Header("Stun")]
    [Tooltip("After a stun ends, the tower ignores new stuns for this many seconds " +
             "(anti stun-lock: a stun never refreshes while active either).")]
    public float stunImmunitySeconds = 3f;
    private float stunTimer;
    private float stunImmunityTimer;

    /// <summary>A stunned tower doesn't shoot, and its DefenderPost releases only footmen.</summary>
    public bool IsStunned => stunTimer > 0f;

    private float fireCooldown = 0f;

    // Scratch buffers for picking a volley's targets, reused so firing never allocates.
    private const int MaxTargets = 4;
    private readonly MonsterMover[] volley = new MonsterMover[MaxTargets];
    private readonly float[] volleySqrDist = new float[MaxTargets];

    // Live registry of standing towers, so monsters ordered to attack towers can
    // find the nearest one without scanning the scene. Mirrors ActiveMonsters.
    private static readonly List<Tower> standing = new List<Tower>();
    public static IReadOnlyList<Tower> StandingTowers => standing;

    /// <summary>Raised once when a tower is destroyed (GameManager pays the bounty).</summary>
    public static event System.Action<Tower> Destroyed;

    /// <summary>Raised on every hit with the damage actually dealt (overkill excluded); GameManager pays plunder for it.</summary>
    public static event System.Action<Tower, int> Damaged;

    // A Tower on the castle itself is the castle's own gun, not a separate
    // defense: it never joins StandingTowers, so Attack Towers never targets it
    // and it can't be destroyed (and hide the castle) on its own. The castle
    // falls only through its Castle HP.
    private bool isCastleGun;

    void Awake()
    {
        health = maxHealth;
        isCastleGun = GetComponent<Castle>() != null;
    }

    void OnEnable()
    {
        if (isCastleGun) return;
        if (!IsDestroyed && !standing.Contains(this)) standing.Add(this);
    }

    void OnDisable()
    {
        standing.Remove(this);
    }

    void Update()
    {
        if (IsDestroyed) return;

        Repair(Time.deltaTime);

        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f) stunImmunityTimer = stunImmunitySeconds;
            return; // stunned: no shooting, and the fire cooldown doesn't tick
        }
        if (stunImmunityTimer > 0f) stunImmunityTimer -= Time.deltaTime;

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            // One volley: a shot at each of the nearest monsters in range, with
            // the cooldown counted once for the whole volley.
            int count = FindClosestMonstersInRange();
            if (count > 0)
            {
                for (int i = 0; i < count; i++) Fire(volley[i]);
                fireCooldown = 1f / fireRate;
            }
        }
    }

    // Alarm repair: while the alarm is up, an undisturbed tower heals itself.
    void Repair(float deltaTime)
    {
        int level = GameManager.AlarmLevel;
        if (level <= 0 || isCastleGun || health >= maxHealth) return;
        if (Time.time - lastHitTime < repairDelaySeconds) return;

        repairCarry += repairPerAlarmLevel * level * deltaTime;
        int whole = Mathf.FloorToInt(repairCarry);
        if (whole <= 0) return;
        repairCarry -= whole;
        health = Mathf.Min(maxHealth, health + whole);
    }

    void Fire(MonsterMover target)
    {
        if (projectilePrefab != null)
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

            // Pooled path (ProjectilePool in the scene) avoids an Instantiate per
            // shot once many towers are firing at once. Falls back to a raw
            // Instantiate if the pool hasn't been added to the scene yet.
            Projectile shot = ProjectilePool.Instance != null
                ? ProjectilePool.Instance.Spawn(projectilePrefab, spawnPos)
                : Instantiate(projectilePrefab, spawnPos, Quaternion.identity).GetComponent<Projectile>();

            if (shot != null) shot.Launch(target, damage);
            else target.TakeDamage(damage); // prefab missing the script — don't lose the shot
        }
        else
        {
            target.TakeDamage(damage); // no projectile assigned: instant hit (old behaviour)
        }
    }

    /// <summary>
    /// Stun this tower for the given time. Ignored while already stunned (no
    /// refreshing) and during the immunity window after a stun, so a group of
    /// Stunners can't lock a tower down permanently.
    /// </summary>
    public void Stun(float seconds)
    {
        if (IsDestroyed || isCastleGun || seconds <= 0f) return;
        if (stunTimer > 0f || stunImmunityTimer > 0f) return;

        stunTimer = seconds;
        BountyPopup.Show(transform.position, "STUNNED", new Color(0.5f, 0.8f, 1f), 4f);
        Debug.Log($"{name} stunned for {seconds:0.#}s.");
    }

    /// <summary>Called by monsters attacking this tower.</summary>
    public void TakeDamage(int amount)
    {
        if (IsDestroyed || isCastleGun) return;
        // Armor first, then overkill: plunder is paid on what actually landed.
        int taken = Armor.Reduce(amount, armor);
        int dealt = Mathf.Min(taken, health);
        health -= taken;
        lastHitTime = Time.time;
        Damaged?.Invoke(this, dealt);
        if (health <= 0) DestroyTower();
    }

    void DestroyTower()
    {
        IsDestroyed = true;
        standing.Remove(this);
        Debug.Log($"{name} destroyed!");
        Destroyed?.Invoke(this);
        // Placeholder: just hide it. Later — swap to a rubble model, play a
        // collapse animation + sound (the "destruction should feel earned" beat).
        gameObject.SetActive(false);
    }

    // Fills `volley` with the nearest monsters in range (up to targetsPerShot),
    // nearest first, and returns how many it found. A small insertion sort over
    // the buffer: at most 4 slots, so no allocation and no full sort.
    int FindClosestMonstersInRange()
    {
        int want = Mathf.Clamp(targetsPerShot, 1, MaxTargets);
        float rangeSqr = range * range;
        int count = 0;

        var monsters = MonsterSpawner.ActiveMonsters;
        for (int i = 0; i < monsters.Count; i++)
        {
            MonsterMover monster = monsters[i];
            if (monster == null) continue;

            float sqrDist = (monster.transform.position - transform.position).sqrMagnitude;
            if (sqrDist > rangeSqr) continue;
            if (count == want && sqrDist >= volleySqrDist[want - 1]) continue; // buffer full, and this one is farther

            // Drop into its sorted place; when full it overwrites the farthest slot.
            int slot = count < want ? count : want - 1;
            while (slot > 0 && volleySqrDist[slot - 1] > sqrDist)
            {
                volley[slot] = volley[slot - 1];
                volleySqrDist[slot] = volleySqrDist[slot - 1];
                slot--;
            }
            volley[slot] = monster;
            volleySqrDist[slot] = sqrDist;
            if (count < want) count++;
        }
        return count;
    }
}