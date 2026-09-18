using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The single point of creation and removal for monsters, the owner of the
/// live-monster registry, AND the object pool.
///
/// WHY THIS EXISTS: nothing else should call Instantiate/Destroy on a monster,
/// and nothing should scan the whole scene to find monsters. Routing every
/// birth and death through here means:
///   - object pooling lives in ONE place (here)
///   - towers query a maintained list instead of FindObjectsByType every shot
///   - horde-strength accounting (planned) has one authoritative place to hook
///
/// POOLING: Spawn() reuses an inactive monster instead of instantiating, and
/// Despawn() deactivates and returns it instead of destroying. A 50-unit swarm
/// therefore causes no Instantiate/Destroy churn (and no GC hitch on mobile)
/// once the pool is warm. Pools are kept PER PREFAB, so this keeps working when
/// different cards summon different monster prefabs.
///
/// Not a generic ObjectPool&lt;T&gt;: MonoBehaviour reuse is specific enough
/// (SetActive drives OnEnable/OnDisable registration, state must be reset via
/// OnSpawn) that a bespoke, readable pool here is clearer than a generic one.
/// </summary>
public class MonsterSpawner : MonoBehaviour
{
    public static MonsterSpawner Instance { get; private set; }

    [Header("Pooling")]
    [Tooltip("Optional: a monster prefab to pre-instantiate at startup so the " +
             "first big swarm doesn't spike. Assign the monster your cards use. " +
             "Only prewarms this exact prefab (pools are per-prefab).")]
    public GameObject prewarmPrefab;
    [Min(0)]
    [Tooltip("How many of prewarmPrefab to create inactive at startup.")]
    public int prewarmCount = 20;

    // One pool of inactive, reusable monsters per prefab.
    private readonly Dictionary<GameObject, Queue<MonsterMover>> pools =
        new Dictionary<GameObject, Queue<MonsterMover>>();

    // Live registry of every ACTIVE monster. Static so registration from a
    // monster's OnEnable never depends on Instance having been set first
    // (sidesteps scene load-order surprises).
    private static readonly List<MonsterMover> active = new List<MonsterMover>();

    /// <summary>Every monster currently alive on the field. Read-only.</summary>
    public static IReadOnlyList<MonsterMover> ActiveMonsters => active;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (prewarmPrefab != null && prewarmCount > 0)
            Prewarm(prewarmPrefab, prewarmCount);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Create a single monster at a position and give it a path. Reuses a
    /// pooled monster when one is available; only instantiates when the pool
    /// for this prefab is empty.
    /// </summary>
    public MonsterMover Spawn(GameObject prefab, Vector3 position, Transform[] waypoints)
    {
        if (prefab == null) return null;
        Queue<MonsterMover> pool = GetPool(prefab);

        // Take a reusable monster; skip any Unity-null entries left over from a
        // scene reload that destroyed pooled objects.
        MonsterMover mover = null;
        while (pool.Count > 0 && mover == null)
            mover = pool.Dequeue();

        if (mover == null)
            mover = CreateNew(prefab);
        if (mover == null)
            return null; // prefab had no MonsterMover; CreateNew logged it

        mover.transform.SetPositionAndRotation(position, Quaternion.identity);
        mover.gameObject.SetActive(true); // OnEnable -> Register (idempotent)
        mover.OnSpawn(waypoints);          // reset HP, waypoint index, enabled
        return mover;
    }

    /// <summary>
    /// Summon everything a card describes: spawnCount monsters, staggered by
    /// spawnInterval so a swarm reads as a stream rather than one blob.
    /// </summary>
    public void SpawnCard(CardDefinition card, Vector3 position, Transform[] waypoints)
    {
        if (card == null || card.monsterPrefab == null)
        {
            Debug.LogWarning("SpawnCard called with a null card or prefab.");
            return;
        }
        StartCoroutine(SpawnGroup(card, position, waypoints));
    }

    private IEnumerator SpawnGroup(CardDefinition card, Vector3 position, Transform[] waypoints)
    {
        int count = Mathf.Max(1, card.spawnCount);
        for (int i = 0; i < count; i++)
        {
            Spawn(card.monsterPrefab, position, waypoints);
            if (card.spawnInterval > 0f && i < count - 1)
                yield return new WaitForSeconds(card.spawnInterval);
        }
    }

    /// <summary>
    /// Remove a monster from the field: deactivate it and return it to its
    /// pool for reuse (instead of destroying it).
    /// </summary>
    public void Despawn(MonsterMover mover)
    {
        if (mover == null) return;

        mover.gameObject.SetActive(false); // OnDisable -> Unregister

        if (mover.SourcePrefab != null)
            GetPool(mover.SourcePrefab).Enqueue(mover);
        else
            Destroy(mover.gameObject); // not pool-tracked; don't leak it
    }

    /// <summary>
    /// Pre-instantiate <paramref name="count"/> inactive monsters of a prefab
    /// so the first burst reuses them instead of instantiating mid-fight.
    /// </summary>
    public void Prewarm(GameObject prefab, int count)
    {
        if (prefab == null) return;
        Queue<MonsterMover> pool = GetPool(prefab);
        for (int i = 0; i < count; i++)
        {
            MonsterMover mover = CreateNew(prefab);
            if (mover == null) return; // bad prefab; CreateNew logged it
            mover.gameObject.SetActive(false); // inactive == available
            pool.Enqueue(mover);
        }
    }

    private Queue<MonsterMover> GetPool(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out Queue<MonsterMover> pool))
        {
            pool = new Queue<MonsterMover>();
            pools[prefab] = pool;
        }
        return pool;
    }

    private MonsterMover CreateNew(GameObject prefab)
    {
        GameObject go = Instantiate(prefab);
        MonsterMover mover = go.GetComponent<MonsterMover>();
        if (mover == null)
        {
            Debug.LogError($"Prefab '{prefab.name}' has no MonsterMover component; " +
                           "pooling requires one. Destroying it.");
            Destroy(go);
            return null;
        }
        mover.SourcePrefab = prefab; // remember its pool for Despawn
        return mover;
    }

    // Monsters call these themselves from OnEnable/OnDisable, so the registry
    // stays correct whether a monster is freshly created or reused from a pool.
    public static void Register(MonsterMover mover)
    {
        if (mover != null && !active.Contains(mover)) active.Add(mover);
    }

    public static void Unregister(MonsterMover mover)
    {
        active.Remove(mover);
    }
}
