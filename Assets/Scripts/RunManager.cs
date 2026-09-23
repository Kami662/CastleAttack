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

        if (transform.parent != null)
        {
            // DontDestroyOnLoad only works on a root GameObject. Warn loudly
            // instead of silently failing to persist across scene reloads.
            Debug.LogWarning("RunManager must be a root GameObject (not nested " +
                             "under GameplayRig or anything else) for DontDestroyOnLoad " +
                             "to work — unparent it in the scene.");
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
}
