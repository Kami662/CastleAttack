using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Scene refs")]
    public Transform spawnPoint;
    public Transform[] waypoints;
    public HandManager hand;

    [Header("Economy")]
    public int currency = 100;

    void Update()
    {
        // Dev shortcut: number keys 1-5 play the matching hand slot.
        // The real input is tapping a card (placeholder UI: HandDebugUI).
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlayCardFromHand(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) PlayCardFromHand(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) PlayCardFromHand(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) PlayCardFromHand(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) PlayCardFromHand(4);
    }

    /// <summary>
    /// Play the card in the given hand slot if it exists and is affordable.
    /// Spends currency, summons the card's monsters, and consumes the card
    /// (discard + draw a replacement). Returns true on success.
    /// </summary>
    public bool PlayCardFromHand(int index)
    {
        if (hand == null) return false;

        CardDefinition card = hand.GetCard(index);
        if (card == null) return false;
        if (currency < card.spawnCost) return false;

        if (MonsterSpawner.Instance == null)
        {
            Debug.LogError("No MonsterSpawner in the scene — cannot play cards.");
            return false;
        }

        currency -= card.spawnCost;
        MonsterSpawner.Instance.SpawnCard(card, spawnPoint.position, waypoints);
        hand.ConsumeCard(index);
        Debug.Log($"Played '{card.cardName}' ({card.spawnCount}x). Currency left: {currency}");
        return true;
    }

    // The player can still make a move if any card in hand is affordable.
    // GameOverManager reads this for the lose check.
    public bool CanPlayAnyCard => hand != null && hand.AnyAffordable(currency);
}
