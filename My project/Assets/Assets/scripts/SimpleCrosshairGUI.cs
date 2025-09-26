using UnityEngine;
using UnityEngine.InputSystem; // for Mouse.current (optional)

public class SimpleCrosshairGUI : MonoBehaviour
{
    [Header("Links")]
    public ProjectileGun gun;           // active weapon (set once or via BindToGun)
    public Camera cam;                  // if null, uses gun.playerCam

    [Header("Look")]
    public Color color = Color.yellow;
    public float length = 14f;          // line length (px)
    public float thickness = 2f;        // line thickness (px)
    public float baseGap = 8f;          // minimum distance from center (px)
    public bool drawCenterDot = true;
    public float dotSize = 3f;

    [Header("Animation")]
    public float extraWhileFiring = 10f; // extra gap while holding fire
    public float lerpSpeed = 18f;        // how fast the gap eases
    public float fireEase = 18f;         // how fast firing bloom eases

    float currentGap;
    float fireBloom;

    void OnGUI()
    {
        if (Event.current.type != EventType.Repaint) return;

        if (!cam && gun) cam = gun.playerCam;
        if (!cam) return;

        // spread (deg) -> pixels on screen using camera FOV
        float spreadDeg = gun ? gun.spreadDegrees : 0f;
        float spreadPx = DegToPixels(spreadDeg, cam);

        // little bloom while firing
        bool firing = Mouse.current != null && Mouse.current.leftButton.isPressed;
        float targetBloom = firing ? extraWhileFiring : 0f;
        fireBloom = Mathf.Lerp(fireBloom, targetBloom, 1f - Mathf.Exp(-fireEase * Time.deltaTime));

        float targetGap = baseGap + spreadPx + fireBloom;
        currentGap = Mathf.Lerp(currentGap, targetGap, 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime));

        // draw
        var tex = Texture2D.whiteTexture;
        var oldCol = GUI.color;
        GUI.color = color;

        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;

        // top
        GUI.DrawTexture(new Rect(cx - thickness * 0.5f, cy - currentGap - length, thickness, length), tex);
        // bottom
        GUI.DrawTexture(new Rect(cx - thickness * 0.5f, cy + currentGap, thickness, length), tex);
        // left
        GUI.DrawTexture(new Rect(cx - currentGap - length, cy - thickness * 0.5f, length, thickness), tex);
        // right
        GUI.DrawTexture(new Rect(cx + currentGap, cy - thickness * 0.5f, length, thickness), tex);

        if (drawCenterDot)
            GUI.DrawTexture(new Rect(cx - dotSize * 0.5f, cy - dotSize * 0.5f, dotSize, dotSize), tex);

        GUI.color = oldCol;
    }

    float DegToPixels(float deg, Camera c)
    {
        if (deg <= 0f || !c) return 0f;
        float ang = Mathf.Tan(deg * Mathf.Deg2Rad);              // angular offset
        float fovy = Mathf.Tan(0.5f * c.fieldOfView * Mathf.Deg2Rad);
        return (ang / Mathf.Max(0.0001f, fovy)) * (Screen.height * 0.5f);
    }

    // Call this when swapping weapons
    public void BindToGun(ProjectileGun newGun)
    {
        gun = newGun;
        if (!cam && gun) cam = gun.playerCam;
    }
}
