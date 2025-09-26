using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;                 // ← add

public class WeaponSpriteOverlay : MonoBehaviour
{
    [Header("Refs")]
    public Camera playerCam;
    public Image img;

    [Header("Sprites")]
    public Sprite idleSprite;
    public Sprite[] fireFrames;
    public float fireFps = 16f;

    [Header("Input (New Input System)")]
    public InputAction fire;

    [Header("Gun + UI")]
    public ProjectileGun gun;               // ← assign your Gun object
    public TextMeshProUGUI ammoText;        // ← assign a TMP text under the Canvas

    [Header("Reload Anim")]
    public float reloadDropPx = 80f;        // how far to drop the gun (UI pixels)
    public AnimationCurve reloadCurve =     // gentle ease in/out
        AnimationCurve.EaseInOut(0,0,1,1);

    bool playing;           // fire animation gate
    bool reloadPlaying;     // reload anim gate
    RectTransform rt;
    Vector2 restPos;

    void Awake()
    {
        if (!img) img = GetComponent<Image>();
        if (!playerCam) playerCam = Camera.main;
        img.sprite = idleSprite;

        rt = (RectTransform)transform;
        restPos = rt.anchoredPosition;     // where the sprite normally sits
    }

    void OnEnable(){ fire.Enable(); }
    void OnDisable(){ fire.Disable(); }

    void Update()
    {
        // Fire animation
        if (!playing && fire.WasPressedThisFrame())
            StartCoroutine(PlayFireOnce());

        // Ammo label
        if (ammoText && gun)
            ammoText.text = gun.IsReloading ? "RELOADING…" 
                                            : $"{gun.AmmoInMag} / {gun.ReserveAmmo}";

        // Start reload drop when gun enters reloading
        if (gun && gun.IsReloading && !reloadPlaying)
            StartCoroutine(PlayReloadDrop(gun.reloadTime));
    }

    System.Collections.IEnumerator PlayFireOnce()
    {
        playing = true;
        float dt = 1f / Mathf.Max(1f, fireFps);
        if (fireFrames != null && fireFrames.Length > 0)
        {
            foreach (var s in fireFrames)
            {
                img.sprite = s;
                yield return new WaitForSeconds(dt);
            }
        }
        img.sprite = idleSprite;
        playing = false;
    }

    System.Collections.IEnumerator PlayReloadDrop(float duration)
    {
        reloadPlaying = true;

        Vector2 start = restPos;
        Vector2 down  = restPos + new Vector2(0f, -reloadDropPx);

        float t = 0f;
        // go down first half, up second half
        while (t < duration)
        {
            float p = Mathf.Clamp01(t / duration);
            float half = 0.5f;

            if (p < half)
            {
                float k = reloadCurve.Evaluate(p / half);
                rt.anchoredPosition = Vector2.LerpUnclamped(start, down, k);
            }
            else
            {
                float k = reloadCurve.Evaluate((p - half) / half);
                rt.anchoredPosition = Vector2.LerpUnclamped(down, start, k);
            }

            t += Time.deltaTime;
            yield return null;
        }

        rt.anchoredPosition = restPos;
        reloadPlaying = false;
    }
}
