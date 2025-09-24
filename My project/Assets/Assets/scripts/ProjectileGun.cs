using UnityEngine;
using UnityEngine.InputSystem;

public class ProjectileGun : MonoBehaviour
{
    [Header("Refs")]
    public Camera playerCam;
    public Transform muzzle;                  // optional, for barrel position
    public GameObject bulletPrefab;           // assign your Bullet prefab here
    public AudioSource fireAudio;             // optional

    [Header("Fire Settings")]
    public bool automatic = true;
    public float fireRate = 10f;              // shots per second
    public float muzzleVelocity = 80f;        // speed of bullet
    public float spreadDegrees = 0.5f;        // small cone of spread
    public float maxLifeSeconds = 3f;         // bullet lifetime

    [Header("Ammo Settings")]
    public int magSize = 30;
    public int reserveAmmo = 120;
    public float reloadTime = 1.2f;

    [Header("Input")]
    public InputAction fire;                  // bind to <Mouse>/leftButton
    public InputAction reload;                // bind to Keyboard/r

    int ammo;
    bool reloading;
    float nextShotAt;

    void Awake()
    {
        if (!playerCam) playerCam = Camera.main;
        ammo = magSize;
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
        if (ammo <= 0)
        {
            StartReload();
            return;
        }

        ammo--;

        if (fireAudio) fireAudio.Play();

        // Direction from camera center with spread
        Transform cam = playerCam.transform;
        Vector3 dir = ApplySpread(cam.forward, spreadDegrees, cam);

        // ✅ Spawn in front of the camera to avoid hitting the player
        Vector3 pos = muzzle
            ? muzzle.position
            : cam.position + cam.forward * 0.5f;

        Quaternion rot = Quaternion.LookRotation(dir);

        GameObject go = Instantiate(bulletPrefab, pos, rot);
        Bullet b = go.GetComponent<Bullet>();
        if (b != null)
        {
            b.Init(dir * muzzleVelocity);
            b.maxLife = maxLifeSeconds;
        }
    }

    Vector3 ApplySpread(Vector3 fwd, float deg, Transform basis)
    {
        if (deg <= 0f) return fwd;
        float rad = deg * Mathf.Deg2Rad;
        Vector2 c = Random.insideUnitCircle * Mathf.Tan(rad);
        return (fwd + c.x * basis.right + c.y * basis.up).normalized;
    }

    void StartReload()
    {
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

    // Expose ammo for UI
    public int AmmoInMag => ammo;
    public int ReserveAmmo => reserveAmmo;
    public bool IsReloading => reloading;
}
