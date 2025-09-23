using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class WeaponSpriteOverlay : MonoBehaviour
{
    [Header("Refs")]
    public Camera playerCam;                    // your FPS camera
    public Image img;                           // the UI Image under the Canvas
    public Sprite idleSprite;                   // resting frame
    public Sprite[] fireFrames;                 // sliced animation frames, in order
    public float fireFps = 16f;                 // playback speed

    [Header("Input (New Input System)")]
    public InputAction fire;                    // Button (Mouse L, RT)

    bool playing;

    void Awake()
    {
        if (!img) img = GetComponent<Image>();
        if (!playerCam) playerCam = Camera.main;
        img.sprite = idleSprite;
    }

    void OnEnable() { fire.Enable(); }
    void OnDisable() { fire.Disable(); }

    void Update()
    {
        // Play once per click; for full-auto, hold will retrigger when done
        if (!playing && fire.WasPressedThisFrame())
            StartCoroutine(PlayFireOnce());
    }

    IEnumerator PlayFireOnce()
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
}
