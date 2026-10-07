using UnityEngine;

/// <summary>
/// A world-space health bar that only appears while its owner is under attack
/// (GDD §12 phase 1): shown when damage lands, hidden again after HideDelay
/// seconds without a hit, so the battlefield stays clean until something is
/// actually being fought.
///
/// Built from two quads in code, like BountyPopup, so it needs no prefab and no
/// art — the designed bar replaces it in the Phase 2 UI pass. Created lazily by
/// Tower/Castle on their first hit, so buildings that are never attacked never
/// pay for one.
/// </summary>
public class HealthBar : MonoBehaviour
{
    /// <summary>Seconds without taking damage before the bar hides itself.</summary>
    public const float HideDelay = 3f;

    // Sized for the current camera distance: the ground is 60 units across, so a
    // world unit is only ~13px on screen and a thinner bar than this reads as a line.
    const float Width = 3.5f;
    const float Height = 0.6f;
    const float ClearanceAboveOwner = 0.8f;

    static readonly Color BackColor = new Color(0.08f, 0.08f, 0.08f, 1f);
    static readonly Color FullColor = new Color(0.25f, 0.85f, 0.25f, 1f);
    static readonly Color EmptyColor = new Color(0.9f, 0.2f, 0.15f, 1f);

    private Transform fill;
    private Renderer fillRenderer;
    private MaterialPropertyBlock fillProps;
    private float hideTimer;

    /// <summary>
    /// Build a bar for this owner, parented to it so it follows and disappears
    /// with the building. Height is taken from the owner's renderer bounds, so it
    /// still sits correctly when the placeholder models are swapped for real art.
    /// </summary>
    public static HealthBar Create(Transform owner)
    {
        float top = TopOf(owner);

        var root = new GameObject("HealthBar");
        root.transform.SetParent(owner, false);
        root.transform.position = new Vector3(owner.position.x, top + ClearanceAboveOwner, owner.position.z);

        var bar = root.AddComponent<HealthBar>();
        bar.Build(root.transform);
        root.SetActive(false); // nothing to show until the first hit
        return bar;
    }

    /// <summary>Show the bar at this fraction (0–1) and restart the hide countdown.</summary>
    public void Set(float fraction)
    {
        fraction = Mathf.Clamp01(fraction);

        // Scale from the left edge: a centred quad has to shift left by half of
        // what it loses, or the bar would shrink towards its middle.
        fill.localScale = new Vector3(Width * fraction, Height, 1f);
        fill.localPosition = new Vector3(-Width * (1f - fraction) * 0.5f, 0f, -0.01f);

        // A property block tints this one bar without instancing a new material
        // per building (which is what setting renderer.material would do).
        fillRenderer.GetPropertyBlock(fillProps);
        fillProps.SetColor("_BaseColor", Color.Lerp(EmptyColor, FullColor, fraction));
        fillRenderer.SetPropertyBlock(fillProps);

        hideTimer = HideDelay;
        if (!gameObject.activeSelf) gameObject.SetActive(true);
    }

    void Update()
    {
        hideTimer -= Time.deltaTime;
        if (hideTimer <= 0f)
        {
            gameObject.SetActive(false);
            return;
        }

        Camera cam = Camera.main;
        if (cam != null) transform.rotation = cam.transform.rotation;
    }

    private void Build(Transform root)
    {
        Transform back = MakeQuad(root, "Back", BackColor);
        back.localScale = new Vector3(Width, Height, 1f);

        fill = MakeQuad(root, "Fill", FullColor);
        fillRenderer = fill.GetComponent<Renderer>();
        fillProps = new MaterialPropertyBlock();
    }

    private static Transform MakeQuad(Transform parent, string name, Color color)
    {
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = name;
        quad.transform.SetParent(parent, false);
        Destroy(quad.GetComponent<Collider>()); // visual only — must not block clicks or shots

        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.SetColor("_BaseColor", color);
        quad.GetComponent<Renderer>().sharedMaterial = mat;
        return quad.transform;
    }

    // Highest point of the owner's visuals, so the bar clears the model rather
    // than sitting at a hardcoded height that breaks when the art changes.
    private static float TopOf(Transform owner)
    {
        var renderers = owner.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return owner.position.y;

        float top = renderers[0].bounds.max.y;
        for (int i = 1; i < renderers.Length; i++)
            top = Mathf.Max(top, renderers[i].bounds.max.y);
        return top;
    }
}
