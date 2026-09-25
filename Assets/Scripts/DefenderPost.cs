using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Put on a tower or the castle: holds a finite reserve of soldiers and releases
/// them, a couple at a time, while a monster is within alert range (GDD §3
/// "Defenders"). Soldiers that die are gone; nothing respawns. If the building
/// falls, the reserve is lost, but soldiers already out keep fighting.
/// </summary>
public class DefenderPost : MonoBehaviour
{
    [Header("Soldiers")]
    public GameObject footmanPrefab;
    public GameObject archerPrefab;
    [Tooltip("Footmen this building holds in total. They never respawn.")]
    public int footmen = 2;
    [Tooltip("Archers this building holds in total (fewer than footmen by design).")]
    public int archers = 1;

    [Header("Alarm (GDD §3 \"Alarm\")")]
    [Tooltip("Footmen added to the reserve each time the alarm level rises. Towers 0; the castle calls for help.")]
    public int reinforcementsPerAlarmLevel = 0;
    [Tooltip("Each alarm level shortens the release interval by this fraction (0.25 = 25% faster per level).")]
    public float alarmReleaseSpeedup = 0.25f;
    private int appliedAlarmLevel;

    [Header("Release")]
    [Tooltip("How many of this building's soldiers can be out at once.")]
    public int maxOut = 2;
    [Tooltip("Minimum seconds between releases.")]
    public float releaseInterval = 2.5f;
    [Tooltip("Soldiers are released while a monster is this close to the building.")]
    public float alertRange = 12f;
    [Tooltip("Soldiers won't chase a monster farther than this from the building.")]
    public float leashRadius = 14f;
    [Tooltip("Soldiers appear this far from the building, in a random direction.")]
    public float spawnRadius = 2.5f;
    [Tooltip("Height soldiers stand at (the placeholder capsule's centre).")]
    public float standHeight = 0.9f;

    private int footmenLeft;
    private int archersLeft;
    private int released;
    private float timer;
    private readonly List<Defender> soldiersOut = new List<Defender>();

    // The castle object carries both a Castle and a Tower (its gun), so which
    // one decides "has this building fallen" depends on Castle being present.
    private Tower tower;
    private Castle castle;

    void Awake()
    {
        footmenLeft = footmen;
        archersLeft = archers;
        tower = GetComponent<Tower>();
        castle = GetComponent<Castle>();
    }

    // The building has fallen: no more releases. A destroyed Tower deactivates
    // itself (so Update stops on its own), but the castle keeps its object.
    bool IsFallen => (castle != null && castle.isDestroyed) ||
                     (castle == null && tower != null && tower.IsDestroyed);

    void Update()
    {
        if (IsFallen) return;

        // Alarm: each new level calls in reinforcements (footmen) for this building.
        int level = GameManager.AlarmLevel;
        if (level > appliedAlarmLevel)
        {
            int added = (level - appliedAlarmLevel) * reinforcementsPerAlarmLevel;
            footmenLeft += added;
            appliedAlarmLevel = level;
            if (added > 0) Debug.Log($"{name} calls for reinforcements: +{added} footmen (alarm {level}).");
        }

        soldiersOut.RemoveAll(d => d == null || !d.IsAlive);

        timer -= Time.deltaTime;
        if (timer > 0f) return;
        if (footmenLeft + archersLeft <= 0) return;
        if (soldiersOut.Count >= maxOut) return;
        if (!MonsterInAlertRange()) return;

        // A raised alarm shortens the wait between releases.
        if (Release()) timer = releaseInterval / (1f + alarmReleaseSpeedup * level);
    }

    bool MonsterInAlertRange()
    {
        float sqrRange = alertRange * alertRange;
        var monsters = MonsterSpawner.ActiveMonsters;
        for (int i = 0; i < monsters.Count; i++)
        {
            MonsterMover m = monsters[i];
            if (m == null || !m.IsAlive) continue;
            if ((m.transform.position - transform.position).sqrMagnitude <= sqrRange) return true;
        }
        return false;
    }

    // Two footmen for every archer, so archers stay the smaller share; falls back
    // to whichever kind is left. A stunned building sends only footmen (its
    // archers can't "shoot from the building"), and sends nothing once its
    // footmen are gone. Returns false if nothing was released.
    bool Release()
    {
        bool stunned = tower != null && tower.IsStunned;

        bool wantArcher = released % 3 == 2;
        if (stunned) wantArcher = false;
        if (wantArcher && archersLeft <= 0) wantArcher = false;
        if (!wantArcher && footmenLeft <= 0)
        {
            if (stunned) return false;
            wantArcher = true;
        }

        GameObject prefab = wantArcher ? archerPrefab : footmanPrefab;
        if (prefab == null)
        {
            Debug.LogWarning($"{name}: DefenderPost has no {(wantArcher ? "archer" : "footman")} prefab assigned.");
            footmenLeft = archersLeft = 0; // don't warn every interval
            return false;
        }

        if (wantArcher) archersLeft--; else footmenLeft--;
        released++;

        Vector2 ring = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 pos = new Vector3(transform.position.x + ring.x, standHeight, transform.position.z + ring.y);

        GameObject go = Instantiate(prefab, pos, Quaternion.identity);
        Defender d = go.GetComponent<Defender>();
        if (d == null) { Destroy(go); return false; }
        d.Init(transform.position, leashRadius);
        soldiersOut.Add(d);
        Debug.Log($"{name} released a {d.kind}{(stunned ? " (stunned: footmen only)" : "")}. " +
                  $"Left in reserve: {footmenLeft} footmen, {archersLeft} archers.");
        return true;
    }
}
