using UnityEngine;

public class Tower : MonoBehaviour
{
    public float range = 6f;
    public float fireRate = 1f; // shots per second
    public int damage = 15;

    private float fireCooldown = 0f;

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        if (fireCooldown <= 0f)
        {
            MonsterMover target = FindClosestMonsterInRange();
            if (target != null)
            {
                target.TakeDamage(damage);
                fireCooldown = 1f / fireRate;
            }
        }
    }

    // Reads the spawner's live registry instead of FindObjectsByType, which
    // scanned the whole scene on every shot. Skips null/destroyed entries
    // defensively so a stale registry entry can never crash a shot.
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
