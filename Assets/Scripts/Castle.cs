using UnityEngine;

public class Castle : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP;
    public bool isDestroyed = false;


    void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int amount)
    {
        if (currentHP <= 0) return;

        currentHP -= amount;
        Debug.Log($"Castle took {amount} damage. HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            currentHP = 0;
            OnCastleDestroyed();
        }
    }

    void OnCastleDestroyed()
    {
        isDestroyed = true;
        Debug.Log("CASTLE DESTROYED — PLAYER WINS!");
    }
}