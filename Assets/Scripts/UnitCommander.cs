using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One order per group of units (GDD §3 "Unit Commands", §8). Every card the
/// player plays creates a UnitGroup; each monster it summons belongs to it, and
/// reads the group's order every frame, so changing a group's order redirects
/// units that are already on the field.
///
/// New groups start with DefaultOrder. The C / T / H dev keys set that default;
/// the placeholder IMGUI panel below (one tap per order) sets a group's order or
/// the default. The real UI is the Phase 2 touch UI (§12.3), where tapping a
/// card's icon selects its group.
/// </summary>
public class UnitCommander : MonoBehaviour
{
    public enum Order { FocusCastle, AttackTowers, Halt }

    /// <summary>The order new groups start with.</summary>
    public static Order DefaultOrder { get; private set; } = Order.FocusCastle;

    private static readonly List<UnitGroup> groups = new List<UnitGroup>();
    /// <summary>Groups that still have units on the field or units left to spawn.</summary>
    public static IReadOnlyList<UnitGroup> Groups => groups;

    [Header("Dev input (placeholder)")]
    public KeyCode attackTowersKey = KeyCode.T;
    public KeyCode focusCastleKey = KeyCode.C;
    public KeyCode haltKey = KeyCode.H;

    [Header("Placeholder group panel (IMGUI)")]
    public bool showPanel = true; // hides the IMGUI panel (the real UI replaces it in Phase 2)

    // Static state survives scene reloads, so reset to the defaults on load.
    void OnEnable()
    {
        DefaultOrder = Order.FocusCastle;
        groups.Clear();
    }

    void Update()
    {
        if (Input.GetKeyDown(attackTowersKey)) SetDefaultOrder(Order.AttackTowers);
        if (Input.GetKeyDown(focusCastleKey)) SetDefaultOrder(Order.FocusCastle);
        if (Input.GetKeyDown(haltKey)) SetDefaultOrder(Order.Halt);

#if UNITY_EDITOR
        // Editor-only test aid: stun every standing tower, to check what a
        // stunned tower does without needing a Stunner to survive the trip.
        if (Input.GetKeyDown(KeyCode.Y))
        {
            foreach (Tower t in Tower.StandingTowers) t.Stun(5f);
        }
#endif

        // Drop groups whose units are all gone.
        for (int i = groups.Count - 1; i >= 0; i--)
            if (groups[i].IsFinished) groups.RemoveAt(i);
    }

    public static void SetDefaultOrder(Order order)
    {
        DefaultOrder = order;
        Debug.Log($"Default order for new groups: {order}");
    }

    /// <summary>Called by MonsterSpawner when a card is played: the new group takes DefaultOrder.</summary>
    public static UnitGroup CreateGroup(CardDefinition card, int totalUnits)
    {
        var group = new UnitGroup(card, totalUnits, DefaultOrder);
        groups.Add(group);
        return group;
    }

    // Throwaway panel, like HandDebugUI: a row per group with one big button per
    // order, plus a row for the default. Every action is one click.
    void OnGUI()
    {
        if (!showPanel) return;

        const float w = 300f, rowH = 34f, btnW = 88f;
        float x = Screen.width - w - 12f;
        float y = 90f;

        GUI.Box(new Rect(x - 6, y - 6, w + 12, 30 + (groups.Count + 1) * (rowH + 18f) + 6), GUIContent.none);
        GUI.Label(new Rect(x, y, w, 24), "Orders (C / T / H set the default)");
        y += 28f;

        DrawOrderRow(new Rect(x, y, w, rowH), "New groups", DefaultOrder, o => SetDefaultOrder(o), btnW);
        y += rowH + 18f;

        foreach (UnitGroup g in groups)
        {
            UnitGroup group = g;
            string title = $"{group.Label}  ({group.Alive} alive)";
            DrawOrderRow(new Rect(x, y, w, rowH), title, group.Order, o => group.Order = o, btnW);
            y += rowH + 18f;
        }
    }

    static void DrawOrderRow(Rect rect, string title, Order current, System.Action<Order> onPick, float btnW)
    {
        GUI.Label(new Rect(rect.x, rect.y - 2f, rect.width, 18f), title);
        string[] names = { "Castle", "Towers", "Halt" };
        for (int i = 0; i < names.Length; i++)
        {
            var order = (Order)i;
            var r = new Rect(rect.x + i * (btnW + 6f), rect.y + 16f, btnW, rect.height - 14f);
            bool selected = order == current;
            GUI.enabled = !selected;
            if (GUI.Button(r, selected ? "[" + names[i] + "]" : names[i])) onPick(order);
            GUI.enabled = true;
        }
    }
}

/// <summary>
/// The units one card play summoned, and the order they follow. Plain class, not a
/// component: it's data shared by monsters (see MonsterMover.Group).
/// </summary>
public class UnitGroup
{
    public readonly CardDefinition Card;
    public UnitCommander.Order Order;

    private readonly int total;
    private int spawned;
    public int Alive { get; private set; }

    public UnitGroup(CardDefinition card, int totalUnits, UnitCommander.Order order)
    {
        Card = card;
        total = totalUnits;
        Order = order;
    }

    public string Label => Card != null ? Card.cardName : "Group";

    /// <summary>True once every unit has spawned and none are left alive.</summary>
    public bool IsFinished => spawned >= total && Alive <= 0;

    public void MemberSpawned() { spawned++; Alive++; }
    public void MemberSkipped() { spawned++; } // a spawn that failed, so the group can still finish
    public void MemberGone() { Alive--; }
}
