using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Scene refs")]
    public Transform spawnPoint;
    public Transform[] waypoints;

    [Header("Economy")]
    public int currency = 100;

    [Header("Card")]
    [Tooltip("Temporary: the single card the spacebar plays. " +
             "Replaced by a real hand of cards later.")]
    public CardDefinition testCard;

    // True while the player still has a move: a card assigned and enough
    // currency to play it. Generalises to "any card in hand is affordable"
    // once there is a real hand. GameOverManager reads this for the lose check.
    public bool CanPlayAnyCard => testCard != null && currency >= testCard.spawnCost;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryPlayCard();
        }
    }

    void TryPlayCard()
    {
        if (testCard == null)
        {
            Debug.LogError("No card assigned to GameManager.testCard.");
            return;
        }

        if (currency < testCard.spawnCost)
        {
            Debug.Log("Not enough currency!");
            return;
        }

        if (MonsterSpawner.Instance == null)
        {
            Debug.LogError("No MonsterSpawner in the scene — add one before playing cards.");
            return;
        }

        currency -= testCard.spawnCost;
        MonsterSpawner.Instance.SpawnCard(testCard, spawnPoint.position, waypoints);
        Debug.Log($"Played '{testCard.cardName}' ({testCard.spawnCount}x). Currency left: {currency}");
    }
}
