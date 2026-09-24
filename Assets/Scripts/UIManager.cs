using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameManager gameManager;
    public Castle castle;

    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI castleHPText;

    void Awake()
    {
        // The HUD boxes are narrow; wrapping would push text onto the line below.
        currencyText.textWrappingMode = TextWrappingModes.NoWrap;
        castleHPText.textWrappingMode = TextWrappingModes.NoWrap;
    }

    void Update()
    {
        string budget = gameManager.BudgetSpent
            ? "spent"
            : Mathf.CeilToInt(gameManager.BudgetRemaining / gameManager.regenPerSecond) + "s";
        currencyText.text = $"Currency: {gameManager.Currency} / {gameManager.holdingCap}   Budget: {budget}";
        castleHPText.text = $"Castle HP: {castle.currentHP}/{castle.maxHP}";
    }
}