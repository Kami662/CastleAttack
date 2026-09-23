using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object pool for tower projectiles — the same shape as MonsterSpawner's
/// pool, applied to a second object type. Spawn() reuses an inactive
/// projectile instead of instantiating; Despawn() deactivates and returns it
/// instead of destroying. Exists because many towers firing at once would
/// otherwise Instantiate/Destroy every single shot, which is fine at low
/// volume but a GC/hitch risk on mobile once tower count and unit counts
/// scale up.
/// </summary>
public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    [Header("Optional prewarm")]
    [Tooltip("A projectile prefab to pre-instantiate at startup so the first " +
             "volley of shots doesn't spike. Only prewarms this exact prefab " +
             "(pools are per-prefab).")]
    public GameObject prewarmPrefab;
    [Min(0)]
    public int prewarmCount = 30;

    // One pool of inactive, reusable projectiles per prefab.
    private readonly Dictionary<GameObject, Queue<Projectile>> pools =
        new Dictionary<GameObject, Queue<Projectile>>();

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
    /// Get a projectile at a position, ready for the caller to Launch() it.
    /// Reuses a pooled instance when one is available; only instantiates when
    /// the pool for this prefab is empty.
    /// </summary>
    public Projectile Spawn(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return null;
        Queue<Projectile> pool = GetPool(prefab);

        Projectile shot = null;
        while (pool.Count > 0 && shot == null)
            shot = pool.Dequeue();

        if (shot == null)
            shot = CreateNew(prefab);
        if (shot == null)
            return null; // prefab had no Projectile component; CreateNew logged it

        shot.transform.SetPositionAndRotation(position, Quaternion.identity);
        shot.gameObject.SetActive(true);
        return shot;
    }

    /// <summary>Return a projectile to its pool instead of destroying it.</summary>
    public void Despawn(Projectile shot)
    {
        if (shot == null) return;

        shot.gameObject.SetActive(false);

        if (shot.SourcePrefab != null)
            GetPool(shot.SourcePrefab).Enqueue(shot);
        else
            Destroy(shot.gameObject); // not pool-tracked; don't leak it
    }

    /// <summary>
    /// Pre-instantiate <paramref name="count"/> inactive projectiles of a
    /// prefab so the first volley reuses them instead of instantiating mid-fight.
    /// </summary>
    public void Prewarm(GameObject prefab, int count)
    {
        if (prefab == null) return;
        Queue<Projectile> pool = GetPool(prefab);
        for (int i = 0; i < count; i++)
        {
            Projectile shot = CreateNew(prefab);
            if (shot == null) return; // bad prefab; CreateNew logged it
            shot.gameObject.SetActive(false);
            pool.Enqueue(shot);
        }
    }

    private Queue<Projectile> GetPool(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out Queue<Projectile> pool))
        {
            pool = new Queue<Projectile>();
            pools[prefab] = pool;
        }
        return pool;
    }

    private Projectile CreateNew(GameObject prefab)
    {
        GameObject go;
        try
        {
            go = Instantiate(prefab);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ProjectilePool: failed to instantiate '{prefab.name}' " +
                           $"({e.GetType().Name}: {e.Message}). The prefab reference is " +
                           "likely broken — re-assign it by dragging the prefab into the " +
                           "field in the Inspector.");
            return null;
        }

        Projectile shot = go.GetComponent<Projectile>();
        if (shot == null)
        {
            Debug.LogError($"Prefab '{prefab.name}' has no Projectile component; " +
                           "pooling requires one. Destroying it.");
            Destroy(go);
            return null;
        }
        shot.SourcePrefab = prefab; // remember its pool for Despawn
        return shot;
    }
}
