/// <summary>
/// Everything that needs to persist across encounters within a run, as a
/// plain serializable POCO kept separate from RunManager's behaviour — so
/// "saving" is just serializing this to JSON later (GDD §4, "design for it
/// now, build later"). Empty shell for now; fields land here as the run
/// layer gets built (deck, horde strength, wave budget, active modifier,
/// meta-progression).
/// </summary>
[System.Serializable]
public class RunState
{
    /// <summary>Bump this whenever RunState's shape changes, so a future
    /// save/load can detect and migrate old saves instead of misreading them.</summary>
    public int saveVersion = 1;
}
