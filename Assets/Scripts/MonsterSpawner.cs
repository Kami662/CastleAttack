using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The single point of creation and removal for monsters, and the owner of
/// the live-monster registry.
///
/// WHY THIS EXISTS: nothing else should call Instantiate/Destroy on a monster,
/// and nothing should scan the whole scene to find monsters. Routing every
/// birth and death through here means:
///   - object pooling can be dropped in later by changing ONLY this file
///   - towers query a maintained list instead of FindObjectsByType every shot
///   - horde-strength accounting (planned) has one authoritative place to hook
///
/// The current Spawn/Despawn just wrap Instantiate/Destroy. That is deliberate:
/// the value right now is that all the call sites are already correct.
/// </summary>
public class MonsterSpawner : MonoBehaviour
{
    public static MonsterSpawner Instance { get; private set; }

    // Live registry of every active monster. Static so registration from a
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

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Create a monster at a position and give it a path.
    /// Later: pull from a pool instead of instantiating.
    /// </summary>
    public MonsterMover Spawn(GameObject prefab, Vector3 position, Transform[] waypoints)
    {
        GameObject go = Instantiate(prefab, position, Quaternion.identity);
        MonsterMover mover = go.GetComponent<MonsterMover>();
        if (mover != null) mover.waypoints = waypoints;
        return mover;
    }

    /// <summary>
    /// Remove a monster from the field.
    /// Later: return it to a pool instead of destroying.
    /// </summary>
    public void Despawn(MonsterMover mover)
    {
        if (mover == null) return;
        Destroy(mover.gameObject);
    }

    // Monsters call these themselves from OnEnable/OnDisable, so the registry
    // stays correct whether a monster is freshly created or (later) reused.
    public static void Register(MonsterMover mover)
    {
        if (mover != null && !active.Contains(mover)) active.Add(mover);
    }

    public static void Unregister(MonsterMover mover)
    {
        active.Remove(mover);
    }
}
