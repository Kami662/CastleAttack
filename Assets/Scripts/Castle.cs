using UnityEngine;

public class Castle : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP;
    [Tooltip("Flat damage cut from every hit that reaches the castle (min 1 per hit).")]
    public int armor;
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

        int taken = Armor.Reduce(amount, armor);
        int dealt = Mathf.Min(taken, currentHP);
        currentHP -= taken;
        Debug.Log($"Castle took {taken} damage ({amount} - armor {armor}). HP: {currentHP}/{maxHP}");

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