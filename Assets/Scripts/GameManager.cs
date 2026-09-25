using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Scene refs")]
    public Transform spawnPoint;
    public Transform[] waypoints;
    public HandManager hand;

    [Header("Economy: plunder (GDD §3 \"Encounter economy\")")]
    [Tooltip("Currency at the start of an encounter. There is no passive income: after " +
             "this, coins come only from damage dealt, bounties and (later) town tribute.")]
    public int startingCurrency = 100;
    [Tooltip("Plunder stops adding once you hold this much. Bounties can go above it. " +
             "Razing towers raises it for the rest of the encounter.")]
    public int holdingCap = 100;
    [Tooltip("Coins earned per point of damage dealt to a tower or the castle.")]
    public float plunderPerDamage = 1f;
    [Tooltip("Paid when a tower is destroyed, on top of the cap.")]
    public int towerBounty = 40;
    [Tooltip("The holding cap rises by this much for each tower destroyed.")]
    public int capPerRazedTower = 20;
    [Tooltip("Castle HP fractions that each raise the plunder rate once crossed.")]
    public float[] castleMilestones = { 0.75f, 0.5f, 0.25f };
    [Tooltip("Plunder rate added per milestone crossed (0.25 = +25%).")]
    public float milestonePlunderBonus = 0.25f;

    public int Currency { get; private set; }
    public int CapBonus { get; private set; }
    public int HoldingCap => holdingCap + CapBonus;
    public int MilestonesReached { get; private set; }
    public float PlunderMultiplier => 1f + MilestonesReached * milestonePlunderBonus;

    // Economy totals, reported at the end of an encounter for tuning.
    public int PlunderEarned { get; private set; }
    public int PlunderWasted { get; private set; }
    public int BountiesEarned { get; private set; }

    // Plunder below one whole coin, carried to the next hit.
    private float plunderCarry;

    // Plunder pop-ups are batched: a 50-unit swarm hits many times a second, and
    // one "+N" per hit would bury the screen.
    const float PopupInterval = 0.6f;
    const float PlunderPopupHeight = 2f;
    private static readonly Color PlunderColor = new Color(0.85f, 0.95f, 0.55f);
    private int popupCoins;
    private Vector3 popupAt;
    private float popupTimer;

    void Awake()
    {
        Currency = startingCurrency;
    }

    void OnEnable()
    {
        Tower.Damaged += OnTowerDamaged;
        Tower.Destroyed += OnTowerDestroyed;
        Castle.Damaged += OnCastleDamaged;
    }

    void OnDisable()
    {
        Tower.Damaged -= OnTowerDamaged;
        Tower.Destroyed -= OnTowerDestroyed;
        Castle.Damaged -= OnCastleDamaged;
    }

    void Update()
    {
        FlushPlunderPopup();

        // Dev shortcut: number keys 1-5 play the matching hand slot.
        // The real input is tapping a card (placeholder UI: HandDebugUI).
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlayCardFromHand(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) PlayCardFromHand(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) PlayCardFromHand(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) PlayCardFromHand(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) PlayCardFromHand(4);
    }

    void OnTowerDamaged(Tower tower, int damageDealt)
    {
        Plunder(damageDealt, tower.transform.position);
    }

    void OnCastleDamaged(Castle castle, int damageDealt)
    {
        Plunder(damageDealt, castle.transform.position);
        if (!castle.isDestroyed) CheckMilestones(castle);
    }

    /// <summary>
    /// Pay coins for damage dealt. Whatever doesn't fit under the cap is lost
    /// (counted in PlunderWasted, so the tuning log shows how often the cap bites).
    /// </summary>
    void Plunder(int damageDealt, Vector3 at)
    {
        plunderCarry += damageDealt * plunderPerDamage * PlunderMultiplier;
        int whole = Mathf.FloorToInt(plunderCarry);
        if (whole <= 0) return;
        plunderCarry -= whole;

        // Bounties can leave Currency above the cap; that's not an error, it just
        // means plunder has no room until you spend down.
        int room = Mathf.Max(0, HoldingCap - Currency);
        int paid = Mathf.Min(whole, room);
        Currency += paid;
        PlunderEarned += paid;
        PlunderWasted += whole - paid;

        if (paid > 0) QueuePlunderPopup(paid, at);
    }

    void OnTowerDestroyed(Tower tower)
    {
        CapBonus += capPerRazedTower;
        Currency += towerBounty; // on top of the cap
        BountiesEarned += towerBounty;
        BountyPopup.Show(tower.transform.position, $"+{towerBounty}   cap +{capPerRazedTower}");
        Debug.Log($"Tower bounty +{towerBounty}, cap now {HoldingCap}. Currency: {Currency}");
    }

    // Each castle HP milestone crossed raises the plunder rate for the rest of
    // the encounter. Paid at the old rate first (Plunder runs before this).
    void CheckMilestones(Castle castle)
    {
        float fraction = (float)castle.currentHP / castle.maxHP;
        while (MilestonesReached < castleMilestones.Length &&
               fraction <= castleMilestones[MilestonesReached])
        {
            MilestonesReached++;
            BountyPopup.Show(castle.transform.position, $"Plunder x{PlunderMultiplier:0.##}",
                             PlunderColor, PlunderPopupHeight + 3f);
            Debug.Log($"Castle milestone {MilestonesReached}/{castleMilestones.Length} — plunder x{PlunderMultiplier:0.##}");
        }
    }

    void QueuePlunderPopup(int coins, Vector3 at)
    {
        if (popupCoins == 0) popupTimer = PopupInterval;
        popupCoins += coins;
        popupAt = at;
    }

    void FlushPlunderPopup()
    {
        if (popupCoins <= 0) return;
        popupTimer -= Time.deltaTime;
        if (popupTimer > 0f) return;

        BountyPopup.Show(popupAt, "+" + popupCoins, PlunderColor, PlunderPopupHeight);
        popupCoins = 0;
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
