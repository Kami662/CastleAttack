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
}
