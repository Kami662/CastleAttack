using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The castles a run can draw from, and the rule for building a route out of them
/// (GDD §3 "Run shape"). Mirrors CardPool: the content is authored as an asset, the
/// selection rule is code.
///
/// The alpha draws a short route of 3, which is why "3 castles in a fixed order"
/// needs no special case — it is just a short route. The branching map later
/// replaces BuildRoute with a graph generator and leaves everything downstream
/// (RunState.route, the encounter loader, the reward flow) untouched.
///
/// Create via: Assets > Create > Castle Attack > Castle Pool
/// </summary>
[CreateAssetMenu(fileName = "NewCastlePool", menuName = "Castle Attack/Castle Pool")]
public class CastlePool : ScriptableObject
{
    [Tooltip("Every castle a run may draw. Tier decides where one can sit in the route.")]
    public List<CastleDefinition> castles = new List<CastleDefinition>();

    [Min(1)]
    [Tooltip("How many castles a run plays. 3 for the alpha; the target run is 8–12.")]
    public int routeLength = 3;

    [Tooltip("Keep the authored order instead of drawing randomly. On for the alpha, so its " +
             "three castles play in their designed order; off once there are enough castles " +
             "for a drawn route to be worth it.")]
    public bool useAuthoredOrder = true;

    /// <summary>
    /// Build the route for one run: routeLength castles, ordered by tier so the run
    /// escalates. Returns fewer than asked if the pool is too small, rather than
    /// repeating a castle.
    /// </summary>
    public List<CastleDefinition> BuildRoute()
    {
        var valid = castles.FindAll(c => c != null);
        var route = new List<CastleDefinition>();
        if (valid.Count == 0)
        {
            Debug.LogError($"{name}: castle pool is empty — a run has nowhere to go.");
            return route;
        }

        int want = Mathf.Min(routeLength, valid.Count);

        if (useAuthoredOrder)
        {
            route.AddRange(valid.GetRange(0, want));
            return route;
        }

        // Draw without repeats, then sort by tier so difficulty still climbs.
        var remaining = new List<CastleDefinition>(valid);
        for (int i = 0; i < want; i++)
        {
            int pick = Random.Range(0, remaining.Count);
            route.Add(remaining[pick]);
            remaining.RemoveAt(pick);
        }
        route.Sort((a, b) => a.tier.CompareTo(b.tier));
        return route;
    }
}
