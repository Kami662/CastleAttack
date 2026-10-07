using UnityEngine;

/// <summary>
/// One castle, as data (D4): which battlefield to load and how tough it is.
/// Authored as an asset like CardDefinition and DeckDefinition, so new castles
/// are made without touching code.
///
/// Castles are a **pool a run draws from**, not a fixed sequence (GDD §3 "Run
/// shape") — the alpha's three-in-a-row is just a short route through that pool,
/// so growing into the branching map later is a generator, not a rewrite.
///
/// Create via: Assets > Create > Castle Attack > Castle
/// </summary>
[CreateAssetMenu(fileName = "NewCastle", menuName = "Castle Attack/Castle")]
public class CastleDefinition : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Shown on the run map and the \"Castle N of M\" indicator.")]
    public string castleName = "New Castle";

    [Tooltip("Which themed region this castle belongs to — the run map moves through regions " +
             "whose battlefields differ in look and size (GDD §3 \"Run shape\"). A plain string " +
             "while only one theme exists; becomes an enum or its own asset once regions are " +
             "actually authored.")]
    public string region = "Default";

    [Min(1)]
    [Tooltip("Rough difficulty, used to order a drawn route so a run escalates. 1 = first castle.")]
    public int tier = 1;

    [Header("Battlefield")]
    [Tooltip("The layout prefab for this castle: path + waypoints, castle, towers, towns, " +
             "ground, road and spawn portal. Everything that differs between castles lives here; " +
             "the managers and UI stay in GameplayRig.")]
    public GameObject layoutPrefab;

    [Header("Castle")]
    [Min(1)] public int castleHP = 100;
    [Min(0)] public int castleArmor = 3;

    [Header("Castle's own gun (every castle defends itself)")]
    [Min(0f)] public float gunRange = 8f;
    [Min(0f)] public float gunFireRate = 1f;
    [Min(0)] public int gunDamage = 15;

    [Header("Tower strength (multipliers on the layout's authored towers)")]
    [Tooltip("Scales each tower's maxHealth. 1 = as authored in the layout.")]
    [Min(0.1f)] public float towerHealthMultiplier = 1f;
    [Min(0.1f)] public float towerDamageMultiplier = 1f;
    [Min(0.1f)] public float towerRangeMultiplier = 1f;

    /// <summary>
    /// Apply this castle's stats to a freshly instantiated layout. Multipliers are
    /// applied to what the layout prefab authored, so a layout stays readable on its
    /// own and a CastleDefinition only says "the same place, but tougher".
    /// Towns are left alone — they are soft economy targets, not part of the defense.
    /// </summary>
    public void ApplyTo(CastleLayout layout)
    {
        if (layout == null) return;

        if (layout.Castle != null)
        {
            layout.Castle.maxHP = castleHP;
            layout.Castle.currentHP = castleHP;
            layout.Castle.armor = castleArmor;

            // The castle's own gun is a Tower component on the same object.
            Tower gun = layout.Castle.GetComponent<Tower>();
            if (gun != null)
            {
                gun.range = gunRange;
                gun.fireRate = gunFireRate;
                gun.damage = gunDamage;
            }
        }

        foreach (Tower t in layout.GetComponentsInChildren<Tower>(true))
        {
            if (t.isTown || t.GetComponent<Castle>() != null) continue; // towns and the castle gun are not towers
            t.maxHealth = Mathf.Max(1, Mathf.RoundToInt(t.maxHealth * towerHealthMultiplier));
            t.damage = Mathf.Max(0, Mathf.RoundToInt(t.damage * towerDamageMultiplier));
            t.range *= towerRangeMultiplier;
            t.ResetHealthToMax(); // Awake already cached the authored maxHealth
        }
    }
}
