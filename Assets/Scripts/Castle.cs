using UnityEngine;

public class Castle : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP;
    public bool isDestroyed = false;


    /// <summary>Raised on every hit with the damage actually dealt (overkill excluded); GameManager pays plunder and checks milestones.</summary>
    public static event System.Action<Castle, int> Damaged;

    void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int amount)
    {
        if (currentHP <= 0) return;

        int dealt = Mathf.Min(amount, currentHP);
        currentHP -= amount;
        Debug.Log($"Castle took {amount} damage. HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            currentHP = 0;
            OnCastleDestroyed();
        }

        // After the destroyed check, so listeners see the final HP and isDestroyed.
        Damaged?.Invoke(this, dealt);
    }

    void OnCastleDestroyed()
    {
        isDestroyed = true;
        Debug.Log("CASTLE DESTROYED — PLAYER WINS!");
    }
}