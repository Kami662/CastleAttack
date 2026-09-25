using TMPro;
using UnityEngine;

/// <summary>
/// A floating "+40" that rises and fades where a tower fell. Placeholder until
/// the designed pop-up exists; built in code so it needs no prefab or scene
/// wiring. Low volume (one per destroyed tower), so it isn't pooled.
/// </summary>
public class BountyPopup : MonoBehaviour
{
    const float Lifetime = 1.4f;
    const float RiseSpeed = 2.5f;
    const float StartHeight = 3f;

    private TextMeshPro text;
    private Color baseColor;
    private float age;

    static readonly Color Gold = new Color(1f, 0.84f, 0.2f);

    /// <summary>Gold by default; plunder and milestone pop-ups pass their own color and height so they don't stack on a bounty.</summary>
    public static void Show(Vector3 worldPosition, string label, Color? color = null, float height = StartHeight)
    {
        var go = new GameObject("BountyPopup");
        go.transform.position = worldPosition + Vector3.up * height;

        var popup = go.AddComponent<BountyPopup>();
        popup.text = go.AddComponent<TextMeshPro>();
        popup.text.text = label;
        popup.text.fontSize = 24;
        popup.text.fontStyle = FontStyles.Bold;
        popup.text.alignment = TextAlignmentOptions.Center;
        popup.text.color = color ?? Gold;
        popup.baseColor = popup.text.color;
        popup.FaceCamera();
    }

    void Update()
    {
        age += Time.deltaTime;
        transform.position += Vector3.up * (RiseSpeed * Time.deltaTime);
        FaceCamera();

        float alpha = 1f - Mathf.Clamp01(age / Lifetime);
        text.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

        if (age >= Lifetime) Destroy(gameObject);
    }

    void FaceCamera()
    {
        Camera cam = Camera.main;
        if (cam != null) transform.rotation = cam.transform.rotation;
    }
}
