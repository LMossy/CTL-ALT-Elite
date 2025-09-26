using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSwitcher : MonoBehaviour
{
    [Tooltip("Index 0 = slot 1 (pistol), index 1 = slot 2 (rifle)")]
    public ProjectileGun[] weapons;

    [Tooltip("Overlay that shows the gun sprites/ammo")]
    public WeaponSpriteOverlay overlay;

    int currentIndex;

    void Start()
    {
        Activate(0); // start on slot 1
    }

    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) Activate(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) Activate(1);
    }

    void Activate(int i)
    {
        if (weapons == null || weapons.Length == 0) return;
        i = Mathf.Clamp(i, 0, weapons.Length - 1);

        for (int k = 0; k < weapons.Length; k++)
        {
            if (weapons[k]) weapons[k].gameObject.SetActive(k == i);
        }

        currentIndex = i;
        if (overlay && weapons[i]) overlay.BindToGun(weapons[i]);

        var xhair = FindObjectOfType<SimpleCrosshairGUI>();
        if (xhair) xhair.BindToGun(weapons[i]);

    }
}
