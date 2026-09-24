using UnityEngine;

/// <summary>
/// Editor-only Scene-view aid: draws the monster path as connected line
/// segments between this object's children (the waypoints, in order), plus
/// a sphere at each one. Draws nothing in a build and has no effect on
/// gameplay — Gizmos only render in the Scene/Game view while selected or
/// gizmos are on. Attach to the "Path" object, whose children are the
/// waypoints in order (Waypoint_00_Spawn .. Waypoint_04_Castle).
/// </summary>
public class PathVisualizer : MonoBehaviour
{
    public Color lineColor = Color.yellow;
    public float waypointRadius = 0.4f;

    void OnDrawGizmos()
    {
        int count = transform.childCount;
        if (count == 0) return;

        Gizmos.color = lineColor;
        for (int i = 0; i < count; i++)
        {
            Vector3 point = transform.GetChild(i).position;
            Gizmos.DrawWireSphere(point, waypointRadius);
            if (i > 0)
                Gizmos.DrawLine(transform.GetChild(i - 1).position, point);
        }
    }
}
