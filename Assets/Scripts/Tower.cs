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

    MonsterMover FindClosestMonsterInRange()
    {
        MonsterMover[] monsters = FindObjectsByType<MonsterMover>(FindObjectsSortMode.None);
        MonsterMover closest = null;
        float closestDist = range;

        foreach (MonsterMover monster in monsters)
        {
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