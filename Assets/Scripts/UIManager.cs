using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameManager gameManager;
    public Castle castle;

    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI castleHPText;

    void Update()
    {
        currencyText.text = $"Currency: {gameManager.currency}";
        castleHPText.text = $"Castle HP: {castle.currentHP}/{castle.maxHP}";
    }
}