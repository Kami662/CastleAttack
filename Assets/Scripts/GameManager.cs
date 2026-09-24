using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Scene refs")]
    public Transform spawnPoint;
    public Transform[] waypoints;
    public HandManager hand;

    [Header("Economy: hybrid budget (GDD §3 \"Encounter pacing\")")]
    [Tooltip("Currency at the start of an encounter.")]
    public int startingCurrency = 50;
    [Tooltip("Currency gained per second while the encounter budget lasts.")]
    public float regenPerSecond = 4f;
    [Tooltip("Regeneration stops adding once you hold this much. Bounties can go above it.")]
    public int holdingCap = 100;
    [Tooltip("Total currency regeneration can produce in one encounter. It keeps draining " +
             "while you sit at the cap, so an encounter can't be stalled indefinitely.")]
    public int encounterBudget = 360;
    [Tooltip("Paid when a tower is destroyed, on top of both the cap and the budget.")]
    public int towerBounty = 40;

    public int Currency { get; private set; }
    public float BudgetRemaining { get; private set; }
    public bool BudgetSpent => BudgetRemaining <= 0f;
    public int BountiesEarned { get; private set; }

    // Regeneration below one whole coin, carried to the next frame.
    private float regenCarry;

    void Awake()
    {
        Currency = startingCurrency;
        BudgetRemaining = encounterBudget;
    }

    void OnEnable() { Tower.Destroyed += OnTowerDestroyed; }
    void OnDisable() { Tower.Destroyed -= OnTowerDestroyed; }

    void Update()
    {
        Regenerate(Time.deltaTime);

        // Dev shortcut: number keys 1-5 play the matching hand slot.
        // The real input is tapping a card (placeholder UI: HandDebugUI).
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlayCardFromHand(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) PlayCardFromHand(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) PlayCardFromHand(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) PlayCardFromHand(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) PlayCardFromHand(4);
    }

    void Regenerate(float deltaTime)
    {
        if (BudgetSpent) return;

        float amount = Mathf.Min(regenPerSecond * deltaTime, BudgetRemaining);
        BudgetRemaining -= amount;

        // At the cap the budget still drains and the regeneration is lost:
        // use it or lose it.
        if (Currency >= holdingCap)
        {
            regenCarry = 0f;
            return;
        }

        regenCarry += amount;
        int whole = Mathf.FloorToInt(regenCarry);
        if (whole > 0)
        {
            regenCarry -= whole;
            Currency = Mathf.Min(Currency + whole, holdingCap);
        }
    }

    void OnTowerDestroyed(Tower tower)
    {
        Currency += towerBounty;
        BountiesEarned += towerBounty;
        BountyPopup.Show(tower.transform.position, "+" + towerBounty);
        Debug.Log($"Tower bounty +{towerBounty}. Currency: {Currency}");
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
        if (Currency < card.spawnCost) return false;

        if (MonsterSpawner.Instance == null)
        {
            Debug.LogError("No MonsterSpawner in the scene — cannot play cards.");
            return false;
        }

        Currency -= card.spawnCost;
        MonsterSpawner.Instance.SpawnCard(card, spawnPoint.position, waypoints);
        hand.ConsumeCard(index);
        Debug.Log($"Played '{card.cardName}' ({card.spawnCount}x). Currency left: {Currency}");
        return true;
    }

    // The player can still make a move if any card in hand is affordable.
    // GameOverManager reads this for the lose check.
    public bool CanPlayAnyCard => hand != null && hand.AnyAffordable(Currency);
}
