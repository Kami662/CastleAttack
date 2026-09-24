using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Targeting")]
    public float range = 6f;
    public float fireRate = 1f; // shots per second
    public int damage = 15;

    [Header("Projectile")]
    [Tooltip("Placeholder shot prefab (a small sphere for now; swap for an arrow later). " +
             "If empty, the tower falls back to instant-hit damage.")]
    public GameObject projectilePrefab;
    [Tooltip("Where shots spawn from. If empty, they spawn at the tower's position.")]
    public Transform firePoint;

    [Header("Health (destructible)")]
    public int maxHealth = 100;
    private int health;
    public bool IsDestroyed { get; private set; }

    private float fireCooldown = 0f;

    // Live registry of standing towers, so monsters ordered to attack towers can
    // find the nearest one without scanning the scene. Mirrors ActiveMonsters.
    private static readonly List<Tower> standing = new List<Tower>();
    public static IReadOnlyList<Tower> StandingTowers => standing;

    /// <summary>Raised once when a tower is destroyed (GameManager pays the bounty).</summary>
    public static event System.Action<Tower> Destroyed;

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

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            MonsterMover target = FindClosestMonsterInRange();
            if (target != null)
            {
                Fire(target);
                fireCooldown = 1f / fireRate;
            }
        }
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

    /// <summary>Called by monsters attacking this tower.</summary>
    public void TakeDamage(int amount)
    {
        if (IsDestroyed || isCastleGun) return;
        health -= amount;
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

    MonsterMover FindClosestMonsterInRange()
    {
        MonsterMover closest = null;
        float closestSqr = range * range;

        var monsters = MonsterSpawner.ActiveMonsters;
        for (int i = 0; i < monsters.Count; i++)
        {
            MonsterMover monster = monsters[i];
            if (monster == null) continue;

            float sqrDist = (monster.transform.position - transform.position).sqrMagnitude;
            if (sqrDist <= closestSqr)
            {
                closestSqr = sqrDist;
                closest = monster;
            }
        }
        return closest;
    }
}