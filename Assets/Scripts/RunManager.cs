using UnityEngine;

/// <summary>
/// Persistent owner of state that spans multiple encounters within a run —
/// deck, horde strength, wave budget, active modifier, meta-progression, as
/// each gets built. Survives scene reloads via DontDestroyOnLoad; GameManager
/// keeps owning only the current encounter (GDD §4, "run state must outlive
/// the encounter scene").
///
/// Deliberately an empty shell right now — RunState holds nothing but a
/// version int. This exists so future systems (HandManager's deck source,
/// horde-strength accounting, etc.) have a concrete place to plug into
/// instead of getting bolted onto GameManager piece by piece.
/// </summary>
public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    public RunState State { get; private set; } = new RunState();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Lives inside the SceneEnvironment prefab so every scene gets one, but
        // DontDestroyOnLoad only works on root objects — detach first, so only
        // this object persists and not the camera/light it's grouped with.
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Begin a run: build the route this run will play and roll the starter deck's
    /// wildcard once, so every castle is played with the same starting identity
    /// (GDD §3 "Alpha run rules"). Called by the menu scene's "Start Run" in Phase 3;
    /// until that exists, an encounter played directly falls back to its own setup
    /// (HandManager.ResolveWildcard, EncounterLoader's fallback castle).
    /// </summary>
    public void StartRun(DeckDefinition starterDeck, CastlePool castlePool)
    {
        State = new RunState();
        if (starterDeck != null) State.wildcard = starterDeck.RollWildcard();
        if (castlePool != null) State.route = castlePool.BuildRoute();

        Debug.Log($"[Run] Started. Wildcard: {(State.wildcard != null ? State.wildcard.cardName : "none")}. " +
                  $"Route: {RouteSummary()}");
    }

    /// <summary>
    /// The castle just fell — advance to the next one. Returns true if the run
    /// continues, false if that was the last castle (the run is won).
    /// </summary>
    public bool AdvanceRoute()
    {
        State.routePosition++;
        if (State.RouteComplete)
        {
            Debug.Log("[Run] Route complete — run won.");
            return false;
        }
        Debug.Log($"[Run] Castle {State.routePosition + 1} of {State.route.Count}: {State.CurrentCastle?.castleName}");
        return true;
    }

    private string RouteSummary()
    {
        if (State.route.Count == 0) return "none";
        var names = new string[State.route.Count];
        for (int i = 0; i < State.route.Count; i++) names[i] = State.route[i] != null ? State.route[i].castleName : "null";
        return string.Join(" → ", names);
    }
}
