using UnityEngine;

public class MonsterMover : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 3f;
    public int damage = 10;

    public int maxHP = 30;
    private int currentHP;

    private int currentWaypointIndex = 0;

    // Which prefab this instance came from. The spawner sets it so Despawn
    // can return the monster to the correct pool. Not shown in the Inspector.
    [HideInInspector] public GameObject SourcePrefab;

    // Register/unregister with the spawner's live-monster registry.
    // Done in OnEnable/OnDisable so it stays correct now that a monster is
    // reused from a pool (deactivate -> reactivate) rather than freshly
    // created and destroyed.
    void OnEnable()
    {
        MonsterSpawner.Register(this);
    }

    void OnDisable()
    {
        MonsterSpawner.Unregister(this);
    }

    void Awake()
    {
        // Sane default for any monster placed directly in a scene for testing.
        // Every spawned monster gets its real reset from OnSpawn() below.
        currentHP = maxHP;
    }

    /// <summary>
    /// Reset all per-life state and start moving. Called by MonsterSpawner on
    /// EVERY spawn -- fresh or reused from the pool.
    ///
    /// THIS IS THE HEART OF POOLING CORRECTNESS. Start() and field initializers
    /// only run when an object is first instantiated, NOT when a pooled object
    /// is reactivated. A monster reused without this reset would come back with
    /// its last HP (often 0) and its last waypoint index (the castle), so it
    /// would look "born already dead" or instantly hit the castle. Anything
    /// that becomes per-life state later (status effects, buffs) must be
    /// cleared here too.
    /// </summary>
    public void OnSpawn(Transform[] wp)
    {
        waypoints = wp;
        currentHP = maxHP;
        currentWaypointIndex = 0;
        enabled = true; // Update() disables this on reaching the castle; re-arm it.
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

    // Removal always goes through the spawner so the monster returns to its
    // pool. Falls back to Destroy if the spawner somehow isn't present.
    void Die()
    {
        if (MonsterSpawner.Instance != null)
            MonsterSpawner.Instance.Despawn(this);
        else
            Destroy(gameObject);
    }
}
