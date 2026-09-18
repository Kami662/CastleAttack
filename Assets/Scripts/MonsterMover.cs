using UnityEngine;

public class MonsterMover : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 3f;
    public int damage = 10;

    public int maxHP = 30;
    private int currentHP;

    private int currentWaypointIndex = 0;

    // Register/unregister with the spawner's live-monster registry.
    // Done in OnEnable/OnDisable so it stays correct if a monster is
    // later reused from a pool (deactivate -> reactivate) rather than
    // freshly created.
    void OnEnable()
    {
        MonsterSpawner.Register(this);
    }

    void OnDisable()
    {
        MonsterSpawner.Unregister(this);
    }

    void Start()
    {
        currentHP = maxHP;
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                OnReachedCastle();
                enabled = false;
            }
        }
    }

    void OnReachedCastle()
    {
        Castle castle = FindAnyObjectByType<Castle>();
        if (castle != null)
        {
            castle.TakeDamage(damage);
        }
        Die();
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
        {
            Die();
        }
    }

    // Removal always goes through the spawner so pooling can be swapped in
    // by changing only MonsterSpawner. Falls back to Destroy if the spawner
    // somehow isn't present.
    void Die()
    {
        if (MonsterSpawner.Instance != null)
            MonsterSpawner.Instance.Despawn(this);
        else
            Destroy(gameObject);
    }
}
