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

    void Awake()
    {
        health = maxHealth;
    }

    void OnEnable()
    {
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
            GameObject go = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            Projectile shot = go.GetComponent<Projectile>();
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
        if (IsDestroyed) return;
        health -= amount;
        if (health <= 0) DestroyTower();
    }

    void DestroyTower()
    {
        IsDestroyed = true;
        standing.Remove(this);
        Debug.Log($"{name} destroyed!");
        // Placeholder: just hide it. Later — swap to a rubble model, play a
        // collapse animation + sound (the "destruction should feel earned" beat).
        gameObject.SetActive(false);
    }

    MonsterMover FindClosestMonsterInRange()
    {
        MonsterMover closest = null;
        float closestDist = range;

        var monsters = MonsterSpawner.ActiveMonsters;
        for (int i = 0; i < monsters.Count; i++)
        {
            MonsterMover monster = monsters[i];
            if (monster == null) continue;

            float dist = Vector3.Distance(transform.position, monster.transform.position);
            if (dist <= closestDist)
            {
                closestDist = dist;
                closest = monster;
            }
        }
        return closest;
    }
}