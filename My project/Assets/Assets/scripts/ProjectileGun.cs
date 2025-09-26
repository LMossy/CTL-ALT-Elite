using UnityEngine;
using UnityEngine.InputSystem;

public class ProjectileGun : MonoBehaviour
{
    [Header("Refs")]
    public Camera playerCam;
    public Transform muzzle;
    public GameObject bulletPrefab;
    public AudioSource fireAudio;

    [Header("Fire Settings")]
    public bool automatic = true;
    public float fireRate = 10f;
    public float muzzleVelocity = 80f;
    public float spreadDegrees = 0.5f;
    public float maxLifeSeconds = 3f;

    [Header("Ammo Settings")]
    public bool unlimitedAmmo = false;   // ← NEW
    public int magSize = 30;
    public int reserveAmmo = 120;
    public float reloadTime = 1.2f;

    [Header("Damage Override (optional)")]
    public int damageOverride = -1;      // if ≥0 set Bullet.damage on spawn

    [Header("Input")]
    public InputAction fire;
    public InputAction reload;

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
        Vector3 dir = ApplySpread(cam.forward, spreadDegrees, cam);
        Vector3 pos = muzzle ? muzzle.position : cam.position + cam.forward * 0.5f;
        Quaternion rot = Quaternion.LookRotation(dir);

        GameObject go = Instantiate(bulletPrefab, pos, rot);
        Bullet b = go.GetComponent<Bullet>();
        if (b != null)
        {
            b.Init(dir * muzzleVelocity);
            b.maxLife = maxLifeSeconds;
            if (damageOverride >= 0) b.damage = damageOverride;   // optional
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

    // UI getters
    public int AmmoInMag => unlimitedAmmo ? int.MaxValue : ammo;
    public int ReserveAmmo => unlimitedAmmo ? int.MaxValue : reserveAmmo;
    public bool IsReloading => reloading;
}
