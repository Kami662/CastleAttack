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
        // The plunder rate only shows once a castle milestone has raised it.
        string plunder = gameManager.MilestonesReached > 0
            ? $"   Plunder x{gameManager.PlunderMultiplier:0.##}"
            : "";
        // Shown only while a razed town is still paying out.
        string tribute = gameManager.TributePending
            ? $"   Tribute +{gameManager.TributePerSecond:0.#}/s"
            : "";
        currencyText.text = $"Currency: {gameManager.Currency} / {gameManager.HoldingCap}{plunder}{tribute}";
        castleHPText.text = $"Castle HP: {castle.currentHP}/{castle.maxHP}";
    }
}