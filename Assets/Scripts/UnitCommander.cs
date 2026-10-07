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

    /// <summary>Any tower or town still standing? Towns register in StandingTowers too
    /// (only the castle's own gun is excluded), so this covers both.</summary>
    public static bool AnyBuildingsLeft => Tower.StandingTowers.Count > 0;

    private static readonly List<UnitGroup> groups = new List<UnitGroup>();
    /// <summary>Groups that still have units on the field or units left to spawn.</summary>
    public static IReadOnlyList<UnitGroup> Groups => groups;

    [Header("Dev input (placeholder)")]
    public KeyCode attackTowersKey = KeyCode.T;
    public KeyCode focusCastleKey = KeyCode.C;
    public KeyCode haltKey = KeyCode.H;

    [Header("Placeholder group panel (IMGUI)")]
    public bool showPanel = true; // hides the IMGUI panel (the real UI replaces it in Phase 2)
    [Tooltip("Start with the panel collapsed to its header. The player can toggle it in-game.")]
    public bool collapsed = false;

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
        // Editor-only test aids, for checking things the defense normally prevents
        // you from reaching. Y: stun every standing tower, without needing a Stunner
        // to survive the trip. K: chip the castle, for its health bar, the plunder
        // milestones and (later) the D7 damage-state feedback.
        if (Input.GetKeyDown(KeyCode.Y))
        {
            foreach (Tower t in Tower.StandingTowers) t.Stun(5f);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            Castle castle = FindAnyObjectByType<Castle>();
            if (castle != null) castle.TakeDamage(10);
        }
#endif

        // Drop groups whose units are all gone.
        for (int i = groups.Count - 1; i >= 0; i--)
            if (groups[i].IsFinished) groups.RemoveAt(i);

        // Once every tower and town is razed there is nothing for Attack Towers to
        // find, and MonsterMover quietly falls through to path-following — so the
        // order would still read "Towers" while the horde walked to the castle.
        // Switch the default over so the panel says what actually happens.
        if (DefaultOrder == Order.AttackTowers && !AnyBuildingsLeft)
        {
            Debug.Log("Every tower and town is down — default order switches to Focus Castle.");
            SetDefaultOrder(Order.FocusCastle);
        }
    }

    public static void SetDefaultOrder(Order order)
    {
        DefaultOrder = order;
        Debug.Log($"Default order for new groups: {order}");
    }

    /// <summary>
    /// Called by MonsterSpawner when a card is played. One group per card: if that
    /// card already has units on the field (or still spawning), the new units join
    /// its group and follow its current order, so playing Big Push twice gives one
    /// Big Push group. Otherwise a new group starts with DefaultOrder.
    /// </summary>
    public static UnitGroup GetOrCreateGroup(CardDefinition card, int units)
    {
        foreach (UnitGroup existing in groups)
        {
            if (existing.Card == card && !existing.IsFinished)
            {
                existing.AddUnits(units);
                return existing;
            }
        }

        var group = new UnitGroup(card, units, DefaultOrder);
        groups.Add(group);
        return group;
    }

    // Throwaway panel, like HandDebugUI: a row per group with one big button per
    // order, plus a row for the default. Every action is one click.
    //
    // Collapsible, because the panel sits over the top-right of the field and can
    // hide what's happening there — the castle and its health bar, in particular.
    void OnGUI()
    {
        if (!showPanel) return;

        const float w = 300f, rowH = 34f, btnW = 88f, toggleW = 26f;
        float x = Screen.width - w - 12f;
        float y = 90f;

        float bodyHeight = collapsed ? 0f : (groups.Count + 1) * (rowH + 18f);
        GUI.Box(new Rect(x - 6, y - 6, w + 12, 30 + bodyHeight + 6), GUIContent.none);

        // Header: title plus a one-click collapse toggle, so it stays touch-legal.
        GUI.Label(new Rect(x, y, w - toggleW, 24),
                  collapsed ? $"Orders ({groups.Count} group{(groups.Count == 1 ? "" : "s")})"
                            : "Orders (C / T / H set the default)");
        if (GUI.Button(new Rect(x + w - toggleW, y - 2f, toggleW, 22f), collapsed ? "+" : "–"))
            collapsed = !collapsed;

        if (collapsed) return;
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
            // With every building razed, Attack Towers has no target and would be
            // flipped straight back by the auto-switch above — so grey it out rather
            // than offer an order that does nothing.
            bool unavailable = order == Order.AttackTowers && !AnyBuildingsLeft;
            GUI.enabled = !selected && !unavailable;
            if (GUI.Button(r, selected ? "[" + names[i] + "]" : names[i])) onPick(order);
            GUI.enabled = true;
        }
    }
}

/// <summary>
/// All of one card's units on the field, and the order they follow. Playing the
/// same card again adds to the group. Plain class, not a component: it's data
/// shared by monsters (see MonsterMover.Group).
/// </summary>
public class UnitGroup
{
    public readonly CardDefinition Card;
    public UnitCommander.Order Order;

    private int total; // grows when the same card is played again (see GetOrCreateGroup)
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

    public void AddUnits(int units) { total += units; }
    public void MemberSpawned() { spawned++; Alive++; }
    public void MemberSkipped() { spawned++; } // a spawn that failed, so the group can still finish
    public void MemberGone() { Alive--; }
}
