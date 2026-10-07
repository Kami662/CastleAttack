/// <summary>
/// Everything that needs to persist across encounters within a run, as a
/// plain serializable POCO kept separate from RunManager's behaviour — so
/// "saving" is just serializing this to JSON later (GDD §4, "design for it
/// now, build later"). Fields land here as the run layer gets built (deck,
/// horde strength, wave budget, active modifier).
///
/// Note: meta-progression unlocks do NOT belong here — they outlive a run,
/// so they persist as their own blob (GDD §3 "Meta-progression").
/// </summary>
[System.Serializable]
public class RunState
{
    /// <summary>Bump this whenever RunState's shape changes, so a future
    /// save/load can detect and migrate old saves instead of misreading them.</summary>
    public int saveVersion = 1;

    /// <summary>
    /// The wildcard rolled for this run (GDD §3 "Alpha run rules"): one card from the
    /// starter deck's options list, fixed for the whole run so every castle is played
    /// with the same starting identity. Null until a run starts.
    ///
    /// Note for save/load: this is an asset reference, so serializing RunState to JSON
    /// will need it written as an asset id/name and resolved on load, not as the object.
    /// </summary>
    public CardDefinition wildcard;

    /// <summary>
    /// The castles this run will play, in order, chosen when the run starts.
    ///
    /// Deliberately a **route** rather than an index into a fixed 1→2→3 list (GDD §3
    /// "Run shape"): the alpha's three castles are simply a short route, so growing
    /// into the branching map of 8–12 is a route *generator* plus a map screen, with
    /// no change to how an encounter loads or reports its result.
    ///
    /// Same save/load caveat as the wildcard: these are asset references, so JSON
    /// serialization will need them stored as ids and resolved on load.
    /// </summary>
    public System.Collections.Generic.List<CastleDefinition> route =
        new System.Collections.Generic.List<CastleDefinition>();

    /// <summary>How far along the route the player is. 0 = the first castle.</summary>
    public int routePosition;

    /// <summary>The castle being played now, or null if the route is finished/empty.</summary>
    public CastleDefinition CurrentCastle =>
        routePosition >= 0 && routePosition < route.Count ? route[routePosition] : null;

    /// <summary>True once every castle on the route has been cleared — the run is won.</summary>
    public bool RouteComplete => routePosition >= route.Count;
}
