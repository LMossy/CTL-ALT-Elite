using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyChase3D : MonoBehaviour
{
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float stopDistance = 1.0f;
    [SerializeField] Transform player;           // drag Player here or use tag
    [SerializeField] string playerTag = "Player";
    [SerializeField] bool faceTarget = true;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p) player = p.transform;
        }
    }

    void FixedUpdate()
    {
        if (!player) return;

        // Horizontal chase on XZ plane (ignore height)
        Vector3 to = player.position - transform.position;
        to.y = 0f;

        if (to.sqrMagnitude > stopDistance * stopDistance)
        {
            Vector3 dir = to.normalized;
            Vector3 vxz = dir * moveSpeed;

            // Keep gravity-controlled Y velocity; set XZ only
            rb.linearVelocity = new Vector3(vxz.x, rb.linearVelocity.y, vxz.z);
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        if (faceTarget && to.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(new Vector3(to.x, 0f, to.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 0.2f);
        }
    }
}
