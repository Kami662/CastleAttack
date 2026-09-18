using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns the runtime card state for one encounter: a draw pile built from a
/// DeckDefinition, the cards currently in hand, and a discard pile. Pure card
/// bookkeeping — it does not spend currency or spawn anything (GameManager
/// does that when a card is played).
///
/// Later, in the roguelite structure, the deck won't be a fixed asset per
/// encounter — RunManager will hand in the run's current (grown) deck. Keeping
/// this component's input as "a list of CardDefinitions" means that swap is
/// small: feed it the run deck instead of a DeckDefinition asset.
/// </summary>
public class HandManager : MonoBehaviour
{
    [Header("Deck")]
    [Tooltip("The starter deck this encounter draws from.")]
    public DeckDefinition startingDeck;

    [Min(1)]
    [Tooltip("How many cards are held in hand at once.")]
    public int handSize = 3;

    private readonly List<CardDefinition> drawPile = new List<CardDefinition>();
    private readonly List<CardDefinition> discardPile = new List<CardDefinition>();
    private readonly List<CardDefinition> hand = new List<CardDefinition>();

    /// <summary>The cards currently held. Read-only; play via GameManager.</summary>
    public IReadOnlyList<CardDefinition> Hand => hand;

    void Start()
    {
        BuildDeck();
        for (int i = 0; i < handSize; i++) DrawOne();
    }

    private void BuildDeck()
    {
        drawPile.Clear();
        discardPile.Clear();
        hand.Clear();

        if (startingDeck != null)
        {
            foreach (CardDefinition c in startingDeck.cards)
                if (c != null) drawPile.Add(c);
        }
        else
        {
            Debug.LogWarning("HandManager has no startingDeck assigned.");
        }

        Shuffle(drawPile);
    }

    private void Shuffle(List<CardDefinition> list)
    {
        // Fisher-Yates.
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // Draw one card into hand. When the draw pile is empty, reshuffle the
    // discard pile back into it (standard deck-cycling).
    private void DrawOne()
    {
        if (drawPile.Count == 0)
        {
            if (discardPile.Count == 0) return; // nothing left anywhere
            drawPile.AddRange(discardPile);
            discardPile.Clear();
            Shuffle(drawPile);
        }

        int last = drawPile.Count - 1;
        hand.Add(drawPile[last]);
        drawPile.RemoveAt(last);
    }

    /// <summary>The card in a given hand slot, or null if the slot is empty.</summary>
    public CardDefinition GetCard(int index)
    {
        if (index < 0 || index >= hand.Count) return null;
        return hand[index];
    }

    /// <summary>
    /// Called by GameManager after a card is successfully played: move it to
    /// the discard pile and draw a replacement.
    /// </summary>
    public void ConsumeCard(int index)
    {
        if (index < 0 || index >= hand.Count) return;
        discardPile.Add(hand[index]);
        hand.RemoveAt(index);
        DrawOne();
    }

    /// <summary>True if any card in hand is affordable at the given currency.</summary>
    public bool AnyAffordable(int currency)
    {
        foreach (CardDefinition c in hand)
            if (c != null && currency >= c.spawnCost) return true;
        return false;
    }
}
