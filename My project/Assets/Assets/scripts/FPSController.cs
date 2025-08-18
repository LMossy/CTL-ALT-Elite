using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Refs")]
    public Camera playerCam;                 // assign your Camera
    CharacterController cc;

    [Header("Movement")]
    public float walkSpeed = 6f;
    public float sprintMultiplier = 1.6f;
    public float jumpHeight = 1.2f;          // meters
    public float gravity = -20f;             // stronger than default for snappy FPS
    public float airControl = 0.6f;          // 0..1

    [Header("Mouse Look")]
    public float mouseSensitivity = 1.2f;    // overall multiplier
    public float minPitch = -85f;
    public float maxPitch = 85f;

    [Header("Input (New Input System)")]
    public InputAction move;                 // Vector2  (WASD)
    public InputAction look;                 // Vector2  (Mouse delta / Right stick)
    public InputAction jump;                 // Button   (Space/A)
    public InputAction sprint;               // Button   (Left Shift/L3)

    // internals
    float pitch;                              // camera X rotation
    float yVel;                               // vertical velocity

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (playerCam == null) playerCam = Camera.main;
    }

    void OnEnable()
    {
        move.Enable(); look.Enable(); jump.Enable(); sprint.Enable();
        Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
    }
    void OnDisable()
    {
        move.Disable(); look.Disable(); jump.Disable(); sprint.Disable();
        Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
    }

    void Update()
    {
        // --- Look ---
        Vector2 lookDelta = look.ReadValue<Vector2>() * mouseSensitivity;
        // mouse Y -> pitch (camera), clamp
        pitch = Mathf.Clamp(pitch - lookDelta.y, minPitch, maxPitch);
        playerCam.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
        // mouse X -> yaw (body)
        transform.Rotate(0f, lookDelta.x, 0f);

        // --- Move (XZ) ---
        Vector2 m = move.ReadValue<Vector2>();
        Vector3 wish = (transform.right * m.x + transform.forward * m.y).normalized;

        float speed = walkSpeed * (sprint.IsPressed() ? sprintMultiplier : 1f);

        // Ground / jump physics
        if (cc.isGrounded)
        {
            yVel = -1f;                                         // stick to ground
            if (jump.WasPressedThisFrame())
                yVel = Mathf.Sqrt(2f * jumpHeight * -gravity);  // v = sqrt(2gh)
        }
        else
        {
            // limited air control
            wish *= airControl;
        }

        // apply gravity
        yVel += gravity * Time.deltaTime;

        // final velocity
        Vector3 vel = wish * speed + Vector3.up * yVel;

        // move using CharacterController (handles sliding)
        cc.Move(vel * Time.deltaTime);
    }
}
