using UnityEngine;

/// <summary>
/// A monster card, defined as data rather than code.
///
/// Every card is a ScriptableObject asset in the project, so new cards are
/// authored in the editor (or by a second programmer) without touching any
/// script. A "boss" card is spawnCount 1; a "swarm" card is spawnCount 50;
/// an "elite" card is 10 — all the same asset type, different numbers.
///
/// Create via: Assets > Create > Castle Attack > Card
/// </summary>
[CreateAssetMenu(fileName = "NewCard", menuName = "Castle Attack/Card")]
public class CardDefinition : ScriptableObject
{
    /// <summary>How often a card shows up as a reward (GDD §3 "Alpha run rules").
    /// Rares are only offered when a win bonus earns them, so they stay special.</summary>
    public enum Rarity { Common, Rare }

    [Header("Identity")]
    public string cardName = "New Card";
    [TextArea] public string description;

    [Tooltip("Reward frequency. Commons fill the normal offer; rares only appear when a " +
             "win bonus puts one in it (the all-towns bonus). Ignored for starter-deck cards.")]
    public Rarity rarity = Rarity.Common;

    [Header("Cost")]
    [Tooltip("Currency spent to play this card. (Horde-strength cost comes later.)")]
    public int spawnCost = 20;

    [Header("What it summons")]
    public GameObject monsterPrefab;

    [Min(1)]
    [Tooltip("How many monsters this card summons.")]
    public int spawnCount = 1;

    [Min(0f)]
    [Tooltip("Seconds between each summon in the group. 0 = all on the same frame. " +
             "A small value staggers a swarm so it reads as a stream, not one blob.")]
    public float spawnInterval = 0.15f;

    [Header("Unit stats (optional override)")]
    [Tooltip("If on, monsters this card summons use the stats below instead of the " +
             "prefab's defaults — so one card can be a tough 'boss' and another a weak " +
             "swarm using the same monster prefab. If off, the prefab's own stats are used.")]
    public bool overrideStats = false;

    [Min(1)]
    [Tooltip("Monster HP (only used when Override Stats is on).")]
    public int unitMaxHP = 30;

    [Min(0)]
    [Tooltip("Damage each monster deals to the castle (only used when Override Stats is on).")]
    public int unitDamage = 10;

    [Min(0.1f)]
    [Tooltip("Move speed (only used when Override Stats is on). Lower = slower, tankier feel.")]
    public float unitSpeed = 3f;

    [Min(0.1f)]
    [Tooltip("Size multiplier on the monster prefab (only used when Override Stats is on). " +
             "Small swarm units read as a mass; a big boss reads as a boss.")]
    public float unitScale = 1f;

    [Header("Tower traits (optional; work with or without Override Stats)")]
    [Min(1f)]
    [Tooltip("Multiplier on the damage these monsters deal to towers and towns (not the castle). " +
             "1 = normal. The Sapper uses 4: a tower breaker.")]
    public float towerDamageMultiplier = 1f;

    [Min(0f)]
    [Tooltip("Seconds a tower is stunned when one of these monsters hits it. 0 = no stun. " +
             "A stunned tower stops shooting and only releases footmen. It doesn't refresh " +
             "while stunned, and is immune for a few seconds afterwards (anti stun-lock).")]
    public float stunSeconds = 0f;
}