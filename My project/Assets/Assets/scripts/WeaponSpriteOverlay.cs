using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;

[Serializable]
public class WeaponSprites
{
    public string name;
    public ProjectileGun gun;        // the gun this set belongs to
    public Sprite idle;
    public Sprite[] fireFrames;
    public float fireFps = 16f;
    public float reloadDropPx = 80f;

    // NEW: UI tuning per weapon
    public bool useSpriteNativeSize = true; // call Image.SetNativeSize()
    public float uiScale = 1f;              // multiply after native size
    public Vector2 uiOffset = Vector2.zero; // pixels from base position
    public bool loopWhileHeld = false;      // for full-auto weapons
}

public class WeaponSpriteOverlay : MonoBehaviour
{
    [Header("Refs")]
    public Camera playerCam;
    public Image img;
    public TextMeshProUGUI ammoText;

    [Header("Input")]
    public InputAction fire;

    [Header("Per-weapon sprite sets")]
    public WeaponSprites[] sets;

    // runtime
    ProjectileGun gun;
    WeaponSprites cur;
    bool playing, reloadPlaying;
    RectTransform rt;
    Vector2 baseRestPos;  // original anchored pos in the layout
    Vector2 restPos;      // current weapon's rest pos (base + offset)

    void Awake()
    {
        if (!img) img = GetComponent<Image>();
        if (!playerCam) playerCam = Camera.main;
        rt = (RectTransform)transform;
        baseRestPos = rt.anchoredPosition;

        if (sets != null && sets.Length > 0 && sets[0].gun != null)
            ApplySet(sets[0]);
    }

    void OnEnable(){ fire.Enable(); }
    void OnDisable(){ fire.Disable(); }

    void Update()
    {
        if (cur == null || gun == null) return;

        // loop for autos, single burst for semis
        if (cur.loopWhileHeld)
        {
            if (!playing && fire.IsPressed() && !gun.IsReloading)
                StartCoroutine(PlayFireLoop());
        }
        else
        {
            if (!playing && fire.WasPressedThisFrame())
                StartCoroutine(PlayFireOnce());
        }

        // ammo label
        if (ammoText)
        {
            if (gun.IsReloading) ammoText.text = "RELOADING…";
            else if (gun.unlimitedAmmo) ammoText.text = "∞";
            else ammoText.text = $"{gun.AmmoInMag} / {gun.ReserveAmmo}";
        }

        // reload dip
        if (gun.IsReloading && !reloadPlaying)
            StartCoroutine(PlayReloadDrop(gun.reloadTime));
    }

    System.Collections.IEnumerator PlayFireOnce()
    {
        playing = true;
        float dt = 1f / Mathf.Max(1f, cur.fireFps);
        if (cur.fireFrames != null && cur.fireFrames.Length > 0)
        {
            foreach (var s in cur.fireFrames)
            {
                img.sprite = s;
                yield return new WaitForSeconds(dt);
            }
        }
        img.sprite = cur.idle;
        playing = false;
    }

    System.Collections.IEnumerator PlayFireLoop()
    {
        playing = true;
        float dt = 1f / Mathf.Max(1f, cur.fireFps);

        while (fire.IsPressed() && gun && !gun.IsReloading)
        {
            if (cur.fireFrames != null && cur.fireFrames.Length > 0)
            {
                for (int i = 0; i < cur.fireFrames.Length; i++)
                {
                    if (!fire.IsPressed() || (gun && gun.IsReloading)) break;
                    img.sprite = cur.fireFrames[i];
                    yield return new WaitForSeconds(dt);
                }
            }
            else yield return null;
        }

        img.sprite = cur.idle;
        playing = false;
    }

    System.Collections.IEnumerator PlayReloadDrop(float duration)
    {
        reloadPlaying = true;

        float drop = cur.reloadDropPx;  // tune per weapon in Inspector
        Vector2 start = restPos;
        Vector2 down  = restPos + new Vector2(0f, -drop);

        float t = 0f;
        while (t < duration)
        {
            float p = Mathf.Clamp01(t / duration);
            float k = p < 0.5f ? Ease(p / 0.5f) : Ease((p - 0.5f) / 0.5f);
            rt.anchoredPosition = (p < 0.5f)
                ? Vector2.LerpUnclamped(start, down, k)
                : Vector2.LerpUnclamped(down, start, k);
            t += Time.deltaTime;
            yield return null;
        }
        rt.anchoredPosition = restPos;
        reloadPlaying = false;

        float Ease(float x) => x * (2f - x); // smooth-ish
    }

    // Called by your WeaponSwitcher when changing guns
    public void BindToGun(ProjectileGun newGun)
    {
        int idx = Array.FindIndex(sets, s => s.gun == newGun);
        if (idx >= 0) ApplySet(sets[idx]);
    }

    void ApplySet(WeaponSprites s)
    {
        cur = s;
        gun = s.gun;
        if (s.idle) img.sprite = s.idle;

        if (s.useSpriteNativeSize) img.SetNativeSize();
        rt.localScale = Vector3.one * Mathf.Max(0.01f, s.uiScale);

        restPos = baseRestPos + s.uiOffset;
        rt.anchoredPosition = restPos;
    }
}
