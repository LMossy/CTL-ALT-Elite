using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // for Canvas / RenderMode

public class ProjectileGun : MonoBehaviour
{
    [Header("Refs")]
    public Camera playerCam;
    public Transform muzzle;                // optional 3D muzzle
    public GameObject bulletPrefab;
    public AudioSource fireAudio;

    [Header("Fire Settings")]
    public bool automatic = true;           // hold to fire vs click
    public float fireRate = 10f;            // shots per second
    public float muzzleVelocity = 80f;
    public float spreadDegrees = 0.5f;      // random cone around aim
    public float maxLifeSeconds = 3f;

    [Header("Ammo Settings")]
    public bool unlimitedAmmo = false;      // pistol = true
    public int magSize = 30;
    public int reserveAmmo = 120;
    public float reloadTime = 1.2f;

    [Header("Damage Override (optional)")]
    public int damageOverride = -1;         // if >= 0, set Bullet.damage

    [Header("Input (New Input System)")]
    public InputAction fire;                // <Mouse>/leftButton
    public InputAction reload;              // <Keyboard>/r

    [Header("UI Muzzle (for 2D overlay)")]
    public RectTransform uiMuzzleMarker;    // child under your weapon Image at barrel tip
    public Canvas uiCanvas;                 // the Canvas holding the Image
    public float uiSpawnDistance = 0.6f;    // meters in front of camera to spawn

    [Header("Aim Raycast")]
    [SerializeField] LayerMask aimMask = ~0;
    [SerializeField] float aimMaxDistance = 5000f;

    // runtime
    int ammo;
    bool reloading;
    float nextShotAt;

    void Awake()
    {
        if (!playerCam) playerCam = Camera.main;
        ammo = unlimitedAmmo ? int.MaxValue : magSize;
    }

    void OnEnable()
    {
        fire.Enable();
        reload.Enable();
        reload.performed += _ => StartReload();
    }

    void OnDisable()
    {
        reload.performed -= _ => StartReload();
        fire.Disable();
        reload.Disable();
    }

    void Update()
    {
        if (reloading) return;

        bool wantsFire = automatic ? fire.IsPressed() : fire.WasPressedThisFrame();
        if (wantsFire && Time.time >= nextShotAt)
        {
            Shoot();
            nextShotAt = Time.time + 1f / Mathf.Max(1f, fireRate);
        }
    }

    void Shoot()
    {
        if (!unlimitedAmmo && ammo <= 0)
        {
            StartReload();
            return;
        }
        if (!unlimitedAmmo) ammo--;

        if (fireAudio) fireAudio.Play();

        Transform cam = playerCam.transform;

        // 1) Find aim point from center (crosshair)
        Vector3 aimPoint;
        if (Physics.Raycast(cam.position, cam.forward, out var hit, aimMaxDistance, aimMask, QueryTriggerInteraction.Ignore))
            aimPoint = hit.point;
        else
            aimPoint = cam.position + cam.forward * aimMaxDistance;

        // 2) Choose spawn position (UI muzzle → 3D muzzle → camera)
        Vector3 spawnPos = GetSpawnPosFromUIOrWorld(cam);

        // 3) Direction from spawn to aim + spread around camera basis
        Vector3 dir = (aimPoint - spawnPos).normalized;
        dir = ApplySpreadAround(dir, spreadDegrees, cam);

        // 4) Spawn bullet
        Quaternion rot = Quaternion.LookRotation(dir);
        GameObject go = Instantiate(bulletPrefab, spawnPos, rot);

        Bullet b = go.GetComponent<Bullet>();
        if (b != null)
        {
            b.Init(dir * muzzleVelocity);
            b.maxLife = maxLifeSeconds;
            if (damageOverride >= 0) b.damage = damageOverride;
        }
    }

    Vector3 GetSpawnPosFromUIOrWorld(Transform cam)
    {
        // If a UI muzzle marker is set, convert it to a ray through the scene
        if (uiMuzzleMarker)
        {
            Camera canvasCam = null;
            if (uiCanvas && uiCanvas.renderMode == RenderMode.ScreenSpaceCamera)
                canvasCam = uiCanvas.worldCamera; // else null for Overlay

            Vector2 screen = RectTransformUtility.WorldToScreenPoint(canvasCam, uiMuzzleMarker.position);
            Ray r = playerCam.ScreenPointToRay(screen);
            return r.origin + r.direction * Mathf.Max(0.05f, uiSpawnDistance);
        }

        // Otherwise use 3D muzzle or fallback to camera-forward
        if (muzzle) return muzzle.position;
        return cam.position + cam.forward * 0.5f;
    }

    Vector3 ApplySpreadAround(Vector3 baseDir, float deg, Transform cam)
    {
        if (deg <= 0f) return baseDir;
        float rad = deg * Mathf.Deg2Rad;
        Vector2 c = Random.insideUnitCircle * Mathf.Tan(rad);
        return (baseDir + c.x * cam.right + c.y * cam.up).normalized;
    }

    void StartReload()
    {
        if (unlimitedAmmo) return; // nothing to reload
        if (reloading || reserveAmmo <= 0 || ammo == magSize) return;

        reloading = true;
        Invoke(nameof(FinishReload), reloadTime);
    }

    void FinishReload()
    {
        int need = magSize - ammo;
        int take = Mathf.Min(need, reserveAmmo);
        ammo += take;
        reserveAmmo -= take;
        reloading = false;
    }

    // Public getters for UI
    public int AmmoInMag => unlimitedAmmo ? int.MaxValue : ammo;
    public int ReserveAmmo => unlimitedAmmo ? int.MaxValue : reserveAmmo;
    public bool IsReloading => reloading;
}
