using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The set of cards that can be offered as a reward after clearing a castle
/// (GDD §3 "Alpha run rules"). Authored as an asset so the reward table is
/// tuned without code, the same way DeckDefinition holds a deck.
///
/// Rarity lives on the card itself (CardDefinition.rarity), not here, so one
/// card can't be common in one pool and rare in another — a card's rarity is
/// part of what it *is*. Per-castle pools are post-alpha; one shared pool now.
///
/// Create via: Assets > Create > Castle Attack > Card Pool
/// </summary>
[CreateAssetMenu(fileName = "NewCardPool", menuName = "Castle Attack/Card Pool")]
public class CardPool : ScriptableObject
{
    [Tooltip("Every card that can be offered as a reward. A card's own Rarity decides " +
             "whether it appears in a normal offer or only via a win bonus.")]
    public List<CardDefinition> cards = new List<CardDefinition>();

    /// <summary>
    /// Build a reward offer: <paramref name="count"/> distinct cards, commons only,
    /// unless <paramref name="includeRare"/> — then one slot is a rare (the all-towns
    /// win bonus). Falls back to commons if the pool has no rares, and returns fewer
    /// than asked if the pool is too small, rather than repeating a card.
    ///
    /// The reward screen (Phase 3) owns how many slots the win bonuses grant; this
    /// just fills however many it asks for.
    /// </summary>
    public List<CardDefinition> BuildOffer(int count, bool includeRare)
    {
        var offer = new List<CardDefinition>();
        if (count <= 0) return offer;

        var commons = new List<CardDefinition>();
        var rares = new List<CardDefinition>();
        foreach (CardDefinition c in cards)
        {
            if (c == null) continue;
            if (c.rarity == CardDefinition.Rarity.Rare) rares.Add(c);
            else commons.Add(c);
        }

        if (includeRare && rares.Count > 0)
            offer.Add(TakeRandom(rares));

        while (offer.Count < count && commons.Count > 0)
            offer.Add(TakeRandom(commons));

        // Pool had too few commons — top up with rares rather than return a short offer.
        while (offer.Count < count && rares.Count > 0)
            offer.Add(TakeRandom(rares));

        return offer;
    }

    /// <summary>A random card from this pool, ignoring rarity. For the "trade one card
    /// for a random different one" reward step (GDD §3 "Alpha run rules").</summary>
    public CardDefinition RandomCard()
    {
        var valid = cards.FindAll(c => c != null);
        if (valid.Count == 0) return null;
        return valid[Random.Range(0, valid.Count)];
    }

    // Removes as it picks, so one offer never shows the same card twice.
    private static CardDefinition TakeRandom(List<CardDefinition> from)
    {
        int i = Random.Range(0, from.Count);
        CardDefinition picked = from[i];
        from.RemoveAt(i);
        return picked;
    }
}
