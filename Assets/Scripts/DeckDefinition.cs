using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A deck, as data: an ordered list of cards. This is a starter deck (§3) in
/// concrete form — authored as an asset, tuned without code.
///
/// Duplicates are allowed and expected: list a card several times for several
/// copies of it in the deck.
///
/// Create via: Assets > Create > Castle Attack > Deck
/// </summary>
[CreateAssetMenu(fileName = "NewDeck", menuName = "Castle Attack/Deck")]
public class DeckDefinition : ScriptableObject
{
    [Tooltip("The cards this deck contains. List a card multiple times for multiple copies.")]
    public List<CardDefinition> cards = new List<CardDefinition>();

    [Header("Wildcard slot (GDD §3 \"Alpha run rules\")")]
    [Tooltip("One of these is rolled in as an extra card when a run starts, so each run leans " +
             "a different way from castle 1 without changing any rules. Leave empty for a deck " +
             "with no wildcard.")]
    public List<CardDefinition> wildcardOptions = new List<CardDefinition>();

    /// <summary>
    /// Pick one wildcard at random, or null if this deck has none. Called once per
    /// run by RunManager (or per encounter when playing outside a run).
    /// </summary>
    public CardDefinition RollWildcard()
    {
        // Skip empty slots so a half-filled list in the Inspector can't roll a null.
        var options = wildcardOptions.FindAll(c => c != null);
        if (options.Count == 0) return null;
        return options[Random.Range(0, options.Count)];
    }
}
