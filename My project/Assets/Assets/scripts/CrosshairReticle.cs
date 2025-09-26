using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CrosshairReticle : MonoBehaviour
{
    [Header("Links")]
    public ProjectileGun gun;          // active weapon (call BindToGun when you swap)
    public Camera cam;                 // if empty, uses gun.playerCam

    [Header("Pieces (4 Images)")]
    public RectTransform top;
    public RectTransform bottom;
    public RectTransform left;
    public RectTransform right;
    public Image centerDot;            // optional, can be null

    [Header("Style")]
    public float barLength = 14f;      // pixels
    public float barThickness = 2f;    // pixels
    public float baseGap = 8f;         // pixels at zero spread
    public bool showCenterDot = true;

    [Header("Animation")]
    public float lerpSpeed = 18f;      // how fast the gap eases
    public float extraWhileFiring = 10f;    // extra pixels when mouse is held
    public float fireEase = 18f;            // how fast the “firing bloom” eases

    float currentGap;
    float fireBloom; // eased 0..extraWhileFiring

    void Awake()
    {
        if (!cam && gun) cam = gun.playerCam;
        SetupBars();
        if (centerDot) centerDot.enabled = showCenterDot;
    }

    void Update()
    {
        if (!cam && gun) cam = gun.playerCam;

        // 1) Convert gun spread (degrees) → screen-space pixels
        float spreadDeg = gun ? gun.spreadDegrees : 0.0f;
        float pxFromSpread = DegToPixels(spreadDeg, cam);

        // 2) Add base gap + optional bloom while firing
        bool firing = Mouse.current != null && Mouse.current.leftButton.isPressed;
        float targetBloom = firing ? extraWhileFiring : 0f;
        fireBloom = Mathf.Lerp(fireBloom, targetBloom, 1f - Mathf.Exp(-fireEase * Time.deltaTime));

        float targetGap = baseGap + pxFromSpread + fireBloom;

        // 3) Ease to target
        currentGap = Mathf.Lerp(currentGap, targetGap, 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime));

        // 4) Apply to bars
        ApplyLayout(currentGap);
        if (centerDot) centerDot.enabled = showCenterDot;
    }

    float DegToPixels(float deg, Camera c)
    {
        if (!c) return 0f;
        float rad = Mathf.Deg2Rad * Mathf.Max(0f, deg);
        // screen-space radius for an angular offset:
        // r_px = (tan(angle) / tan(fovY/2)) * (Screen.height/2)
        float r = Mathf.Tan(rad);
        float f = Mathf.Tan(0.5f * c.fieldOfView * Mathf.Deg2Rad);
        float px = (r / Mathf.Max(0.0001f, f)) * (Screen.height * 0.5f);
        return px;
    }

    void SetupBars()
    {
        // Ensure pivots are centered so anchoredPosition moves them outwards
        RectTransform[] rts = { top, bottom, left, right };
        foreach (var rt in rts)
        {
            if (!rt) continue;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        }
        // Size bars once; we only move them each frame
        if (top)    top.sizeDelta    = new Vector2(barThickness, barLength);
        if (bottom) bottom.sizeDelta = new Vector2(barThickness, barLength);
        if (left)   left.sizeDelta   = new Vector2(barLength,   barThickness);
        if (right)  right.sizeDelta  = new Vector2(barLength,   barThickness);
    }

    void ApplyLayout(float gap)
    {
        if (top)    top.anchoredPosition    = new Vector2(0f,  +gap);
        if (bottom) bottom.anchoredPosition = new Vector2(0f,  -gap);
        if (left)   left.anchoredPosition   = new Vector2(-gap, 0f);
        if (right)  right.anchoredPosition  = new Vector2(+gap, 0f);
    }

    // Call this from your WeaponSwitcher when swapping guns
    public void BindToGun(ProjectileGun newGun)
    {
        gun = newGun;
        if (!cam && gun) cam = gun.playerCam;
    }
}
