using UnityEngine;

/// <summary>
/// Loads the battlefield for this encounter (D4): picks which castle to play,
/// instantiates its layout prefab, applies the castle's stats, and hands the
/// managers the spawn point, path and castle it just created.
///
/// Two ways in, which is the point:
///  - **In a run:** the castle comes from RunManager's route, so the encounter
///    scene is reused for every castle and only the layout swaps.
///  - **Pressing Play straight into a scene** (how SampleScene and Sandbox are
///    used all day): no run exists, so it falls back to `fallbackCastle` — the
///    "castle picker for testing one castle directly" from D4. Change that field
///    to test any castle without going through the menu.
///
/// Runs in Awake, before GameManager/MonsterSpawner read their references in
/// Start, so the wiring is in place by the time anything uses it.
/// </summary>
[DefaultExecutionOrder(-100)]
public class EncounterLoader : MonoBehaviour
{
    [Header("Managers to wire up")]
    public GameManager gameManager;
    public GameOverManager gameOverManager;
    public UIManager uiManager;

    [Header("Playing this scene directly (no run)")]
    [Tooltip("The castle loaded when there's no run in progress — i.e. whenever you press " +
             "Play straight into this scene. Swap it to test a different castle.")]
    public CastleDefinition fallbackCastle;

    [Tooltip("Where the layout is parented. Leave empty to spawn it at the scene root.")]
    public Transform layoutParent;

    /// <summary>The layout loaded for this encounter.</summary>
    public CastleLayout Layout { get; private set; }

    /// <summary>The castle definition being played.</summary>
    public CastleDefinition Castle { get; private set; }

    void Awake()
    {
        Castle = ResolveCastle();
        if (Castle == null)
        {
            Debug.LogError("EncounterLoader: no castle to load — assign a Fallback Castle, " +
                           "or start a run before loading the encounter scene.");
            return;
        }

        if (Castle.layoutPrefab == null)
        {
            Debug.LogError($"EncounterLoader: castle '{Castle.castleName}' has no layout prefab assigned.");
            return;
        }

        GameObject instance = Instantiate(Castle.layoutPrefab, layoutParent);
        instance.name = Castle.layoutPrefab.name; // drop the "(Clone)" so the hierarchy stays readable

        Layout = instance.GetComponent<CastleLayout>();
        if (Layout == null)
        {
            Debug.LogError($"EncounterLoader: layout prefab '{Castle.layoutPrefab.name}' has no CastleLayout component.");
            return;
        }

        Castle.ApplyTo(Layout);
        WireManagers();

        Debug.Log($"[Encounter] Loaded '{Castle.castleName}' (region {Castle.region}, tier {Castle.tier}).");
    }

    // A run's current castle if there is a run; otherwise the fallback, so pressing
    // Play into this scene keeps working exactly as it did before D4.
    private CastleDefinition ResolveCastle()
    {
        CastleDefinition fromRun = RunManager.Instance != null ? RunManager.Instance.State.CurrentCastle : null;
        if (fromRun != null) return fromRun;

        if (fallbackCastle != null)
            Debug.Log($"[Encounter] No run in progress — loading the fallback castle '{fallbackCastle.castleName}'.");
        return fallbackCastle;
    }

    // The managers used to hold Inspector references into the layout. Now the layout
    // arrives at runtime, so they are handed its spawn point, path and castle here —
    // one place that knows about both halves, instead of each manager hunting for them.
    private void WireManagers()
    {
        if (gameManager != null)
        {
            gameManager.spawnPoint = Layout.spawnPoint;
            gameManager.waypoints = Layout.Waypoints;
        }
        if (gameOverManager != null) gameOverManager.castle = Layout.Castle;
        if (uiManager != null) uiManager.castle = Layout.Castle;
    }
}
