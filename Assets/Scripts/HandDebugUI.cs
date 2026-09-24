using UnityEngine;

/// <summary>
/// TEMPORARY placeholder hand UI, drawn with IMGUI (OnGUI): a row of buttons
/// along the bottom of the screen, one per card in hand, greyed out when the
/// card is unaffordable. Tapping a button plays that card.
///
/// This is dev-only and intentionally ugly. The real card hand is a uGUI/TMP
/// layout designed in the UI/UX pass. Delete this component and its GameObject
/// once the real hand UI exists — nothing else depends on it.
/// </summary>
public class HandDebugUI : MonoBehaviour
{
    public GameManager gameManager;
    public HandManager hand;

    void OnGUI()
    {
        if (gameManager == null || hand == null) return;

        var cards = hand.Hand;
        if (cards.Count == 0) return;

        const float w = 170f, h = 74f, pad = 10f;
        float totalW = cards.Count * w + (cards.Count - 1) * pad;
        float x = (Screen.width - totalW) * 0.5f;
        float y = Screen.height - h - 20f;

        GUILayout.BeginArea(new Rect(0, y - 24, Screen.width, 20));
        GUILayout.Label($"  Currency: {gameManager.Currency}   (placeholder hand — press 1/2/3 or click)");
        GUILayout.EndArea();

        for (int i = 0; i < cards.Count; i++)
        {
            CardDefinition c = cards[i];
            if (c == null) continue;

            bool affordable = gameManager.Currency >= c.spawnCost;
            GUI.enabled = affordable;

            string label = $"{c.cardName}\ncost {c.spawnCost}   ({c.spawnCount}x)\n[key {i + 1}]";
            var rect = new Rect(x + i * (w + pad), y, w, h);
            if (GUI.Button(rect, label))
                gameManager.PlayCardFromHand(i);

            GUI.enabled = true;
        }
    }
}
