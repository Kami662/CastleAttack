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
    [Header("Identity")]
    public string cardName = "New Card";
    [TextArea] public string description;

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
}
