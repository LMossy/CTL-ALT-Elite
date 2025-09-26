using UnityEngine;
using UnityEngine.InputSystem;

public class HitScanGun : MonoBehaviour
{
    [Header("Refs")]
    public Camera playerCam;
    public WeaponSpriteOverlay overlay; // optional; call to animate muzzle frames
    public ParticleSystem muzzleFlash;  // optional
    public AudioSource fireAudio;       // optional
    public GameObject hitDecal;         // optional bullet hole (quad)
    public GameObject hitVFX;           // optional sparks/dust prefab

    [Header("Shooting")]
    public bool automatic = true;
    public float damage = 25f;
    public float fireRate = 10f;      // rounds/sec for auto
    public float range = 250f;
    public float spreadDegrees = 0.4f;
    public LayerMask hitMask = ~0;    // exclude Player layer if needed

    [Header("Ammo")]
    public int magSize = 30;
    public int reserveAmmo = 120;
    public float reloadTime = 1.2f;

    [Header("Input (New Input System)")]
    public InputAction fire;          // Button
    public InputAction reload;        // Button

    int ammo;
    float nextShotAt;
    bool reloading;

    void Awake()
    {
        if (!playerCam) playerCam = Camera.main;
        ammo = magSize;
    }
    void OnEnable() { fire.Enable(); reload.Enable(); reload.performed += OnReload; }
    void OnDisable(){ reload.performed -= OnReload; fire.Disable(); reload.Disable(); }

    void Update()
    {
        if (reloading) return;
        bool wantsFire = automatic ? fire.IsPressed() : fire.WasPressedThisFrame();
        if (wantsFire && Time.time >= nextShotAt)
        {
            Shoot();
            nextShotAt = Time.time + 1f/Mathf.Max(1f, fireRate);
        }
    }

    void Shoot()
    {
        if (ammo <= 0) { StartReload(); return; }
        ammo--;

        overlay?.TriggerFire();                 // animate your 2D weapon sprite
        if (muzzleFlash) muzzleFlash.Play();
        if (fireAudio) fireAudio.Play();

        // ray from camera center with a tiny cone spread
        Transform cam = playerCam.transform;
        Vector3 dir = ApplySpread(cam.forward, spreadDegrees, cam);
        Ray ray = new Ray(cam.position, dir);

        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            // damage if target implements Damageable/Health
            var dmg = hit.collider.GetComponentInParent<Damageable>();
            if (dmg != null) dmg.TakeDamage(damage, hit.point, hit.normal);

            // decal + vfx (optional)
            if (hitDecal)
            {
                var d = Instantiate(hitDecal, hit.point + hit.normal*0.001f, Quaternion.LookRotation(hit.normal));
                d.transform.SetParent(hit.collider.transform, true);
            }
            if (hitVFX)
            {
                var v = Instantiate(hitVFX, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(v, 3f);
            }
        }
    }

    static Vector3 ApplySpread(Vector3 fwd, float deg, Transform basis)
    {
        if (deg <= 0f) return fwd;
        float rad = deg * Mathf.Deg2Rad;
        Vector2 c = Random.insideUnitCircle * Mathf.Tan(rad);
        return (fwd + c.x * basis.right + c.y * basis.up).normalized;
    }

    void OnReload(InputAction.CallbackContext _)
    {
        if (!reloading && ammo < magSize && reserveAmmo > 0) StartReload();
    }
    void StartReload()
    {
        if (reserveAmmo <= 0 || ammo == magSize) return;
        reloading = true; Invoke(nameof(FinishReload), reloadTime);
    }
    void FinishReload()
    {
        int need = magSize - ammo;
        int take = Mathf.Min(need, reserveAmmo);
        ammo += take; reserveAmmo -= take;
        reloading = false;
    }

    // (optional) expose for HUD
    public int AmmoInMag => ammo;
    public int Reserve => reserveAmmo;
    public bool IsReloading => reloading;
}
