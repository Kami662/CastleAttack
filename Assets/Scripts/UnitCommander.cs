using UnityEngine;

/// <summary>
/// The current order for the whole horde. One global order for now, toggled by
/// dev keys; the mobile design (GDD §8) is per-card-group selection, which will
/// replace the keys later without changing how monsters read the order.
/// </summary>
public class UnitCommander : MonoBehaviour
{
    public enum Order { FocusCastle, AttackTowers }

    public static Order Current { get; private set; } = Order.FocusCastle;

    [Header("Dev input (placeholder)")]
    public KeyCode attackTowersKey = KeyCode.T;
    public KeyCode focusCastleKey = KeyCode.C;

    // Static state survives scene reloads, so reset to the default on load.
    void OnEnable() { Current = Order.FocusCastle; }

    void Update()
    {
        if (Input.GetKeyDown(attackTowersKey)) SetOrder(Order.AttackTowers);
        if (Input.GetKeyDown(focusCastleKey)) SetOrder(Order.FocusCastle);
    }

    public static void SetOrder(Order order)
    {
        Current = order;
        Debug.Log($"Order: {order}");
    }
}