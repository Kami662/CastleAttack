using UnityEngine;

/// <summary>
/// Sits on the root of a castle layout prefab and exposes the few things the
/// managers need from a battlefield: where monsters spawn, the path they walk,
/// and which object is the castle (D4).
///
/// Everything that differs between castles lives in the layout prefab — path and
/// waypoints, castle, towers, towns, ground, road, spawn portal. The managers and
/// UI stay in GameplayRig, which is why those two can be swapped independently:
/// one layout per castle, one rig for the whole game.
///
/// Waypoints are read from the Path transform's children in hierarchy order, so a
/// castle's route is authored by arranging children rather than by maintaining a
/// list in the Inspector that silently drifts out of order.
/// </summary>
public class CastleLayout : MonoBehaviour
{
    [Header("Wiring (assign in the layout prefab)")]
    [Tooltip("Where monsters appear.")]
    public Transform spawnPoint;

    [Tooltip("Parent of the waypoints. Its children, in order, are the path.")]
    public Transform path;

    [SerializeField]
    [Tooltip("This layout's castle. Found automatically if left empty.")]
    private Castle castle;

    /// <summary>This layout's castle (the encounter's win condition).</summary>
    public Castle Castle => castle;

    /// <summary>The path, in order. Built from the Path transform's children.</summary>
    public Transform[] Waypoints { get; private set; }

    void Awake()
    {
        if (castle == null) castle = GetComponentInChildren<Castle>(true);
        BuildWaypoints();
    }

    private void BuildWaypoints()
    {
        if (path == null)
        {
            Debug.LogError($"{name}: CastleLayout has no Path assigned — monsters will have nowhere to walk.");
            Waypoints = new Transform[0];
            return;
        }

        Waypoints = new Transform[path.childCount];
        for (int i = 0; i < path.childCount; i++) Waypoints[i] = path.GetChild(i);

        if (Waypoints.Length == 0)
            Debug.LogError($"{name}: the Path has no waypoints under it.");
    }

    // Catches the most common authoring mistakes while editing the prefab, rather
    // than at runtime when the symptom is just "monsters stand still".
    void OnValidate()
    {
        if (spawnPoint == null) Debug.LogWarning($"{name}: CastleLayout has no Spawn Point assigned.", this);
        if (path == null) Debug.LogWarning($"{name}: CastleLayout has no Path assigned.", this);
    }
}
