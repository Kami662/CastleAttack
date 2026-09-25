using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A soldier released by a tower or the castle (GDD §3 "Defenders"). A footman
/// chases the nearest monster near its post and fights it in melee; an archer
/// stays near the post and shoots. Monsters fight back through
/// MonsterMover.FightNearestDefender, which is what makes footmen real blockers.
///
/// Not pooled, unlike monsters: the counts are small (a finite reserve per
/// building) and never respawn. For the same reason there is no per-life reset
/// like MonsterMover.OnSpawn — Init() runs once on a fresh instance.
/// </summary>
public class Defender : MonoBehaviour
{
    public enum Kind { Footman, Archer }

    [Header("Stats")]
    public Kind kind = Kind.Footman;
    public int maxHealth = 40;
    public int damage = 8;
    public float attacksPerSecond = 1f;
    public float speed = 3.5f;
    [Tooltip("Footman: how close it must be to hit. Archer: shooting range.")]
    public float attackRange = 1.5f;

    [Header("Archer")]
    [Tooltip("Shot prefab (same placeholder as the towers). Empty = instant hit.")]
    public GameObject projectilePrefab;

    // Live registry, mirroring ActiveMonsters / StandingTowers, so monsters can
    // find defenders without scanning the scene.
    private static readonly List<Defender> active = new List<Defender>();
    public static IReadOnlyList<Defender> Active => active;

    public bool IsAlive { get; private set; }

    private int health;
    private float cooldown;
    private Vector3 home;
    private float leash;

    void OnEnable() { if (!active.Contains(this)) active.Add(this); }
    void OnDisable() { active.Remove(this); }

    /// <summary>Called once by DefenderPost right after it spawns this soldier.</summary>
    public void Init(Vector3 homePosition, float leashRadius)
    {
        home = homePosition;
        leash = leashRadius;
        health = maxHealth;
        cooldown = 0f;
        IsAlive = true;
    }

    void Update()
    {
        if (!IsAlive) return;

        cooldown -= Time.deltaTime;

        MonsterMover target = FindTarget();
        if (target == null)
        {
            MoveToward(home, 1f);
            return;
        }

        Vector3 targetPos = target.transform.position;
        float sqrDist = FlatSqrDistance(targetPos, transform.position);

        if (sqrDist > attackRange * attackRange)
        {
            // An archer only closes in until it's in range; both stay on the leash.
            MoveToward(targetPos, attackRange * 0.9f);
            return;
        }

        if (cooldown <= 0f)
        {
            Attack(target);
            cooldown = 1f / Mathf.Max(0.01f, attacksPerSecond);
        }
    }

    // Nearest live monster within the leash of the post (not of the soldier),
    // so soldiers defend their building instead of chasing across the map.
    MonsterMover FindTarget()
    {
        MonsterMover best = null;
        float bestSqr = Mathf.Infinity;
        float leashSqr = leash * leash;

        var monsters = MonsterSpawner.ActiveMonsters;
        for (int i = 0; i < monsters.Count; i++)
        {
            MonsterMover m = monsters[i];
            if (m == null || !m.IsAlive) continue;

            Vector3 p = m.transform.position;
            if (FlatSqrDistance(p, home) > leashSqr) continue;

            float sqr = FlatSqrDistance(p, transform.position);
            if (sqr < bestSqr) { bestSqr = sqr; best = m; }
        }
        return best;
    }

    void Attack(MonsterMover target)
    {
        if (kind == Kind.Archer && projectilePrefab != null && ProjectilePool.Instance != null)
        {
            Projectile shot = ProjectilePool.Instance.Spawn(projectilePrefab, transform.position);
            if (shot != null) { shot.Launch(target, damage); return; }
        }
        target.TakeDamage(damage); // footman melee, or no projectile available
    }

    // Moves on the ground plane only: soldiers keep their own height.
    void MoveToward(Vector3 destination, float stopDistance)
    {
        Vector3 flatDest = new Vector3(destination.x, transform.position.y, destination.z);
        if ((flatDest - transform.position).sqrMagnitude <= stopDistance * stopDistance) return;
        transform.position = Vector3.MoveTowards(transform.position, flatDest, speed * Time.deltaTime);
    }

    static float FlatSqrDistance(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x, dz = a.z - b.z;
        return dx * dx + dz * dz;
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive) return;
        health -= amount;
        if (health <= 0)
        {
            IsAlive = false;
            Destroy(gameObject);
        }
    }
}
