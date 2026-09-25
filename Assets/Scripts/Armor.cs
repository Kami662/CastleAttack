using UnityEngine;

/// <summary>
/// Flat armor: every hit is cut by the target's armor, but never below
/// MinimumDamage (GDD §3 "Armor"). Flat rather than a percentage on purpose:
/// it shrinks weak hits (grunts, swarm) much more than strong ones (Brute,
/// Sapper), so which units you play matters. Shared by Tower and Castle so
/// they can't drift apart.
/// </summary>
public static class Armor
{
    /// <summary>A hit that armor would reduce to nothing still chips for this much.</summary>
    public const int MinimumDamage = 1;

    public static int Reduce(int hit, int armor)
    {
        if (hit <= 0) return 0;
        return Mathf.Max(MinimumDamage, hit - armor);
    }
}
