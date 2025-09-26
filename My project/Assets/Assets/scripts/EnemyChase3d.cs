using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyChase3D : MonoBehaviour
{
    [Header("Chase Settings")]
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float stopDistance = 1.0f;
    [SerializeField] Transform player;
    [SerializeField] string playerTag = "Player";
    [SerializeField] bool faceTarget = true;

    [Header("Spawn Settings")]
    [SerializeField] Vector3 spawnCenter = Vector3.zero;         // middle of spawn zone
    [SerializeField] Vector3 spawnSize = new Vector3(40, 1, 40); // area size
    [SerializeField] float minDistanceFromPlayer = 5f;           // avoid spawning on player

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Start()
    {
        // Find player if not assigned
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p) player = p.transform;
        }

        // Pick a random spawn point
        Vector3 pos;
        int safety = 0;
        do
        {
            pos = spawnCenter + new Vector3(
                Random.Range(-spawnSize.x / 2, spawnSize.x / 2),
                Random.Range(-spawnSize.y / 2, spawnSize.y / 2),
                Random.Range(-spawnSize.z / 2, spawnSize.z / 2)
            );
            safety++;
        }
        while (player && Vector3.Distance(pos, player.position) < minDistanceFromPlayer && safety < 20);

        transform.position = pos;
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

    void OnDrawGizmosSelected()
    {
        // visualize spawn box in Scene view
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(spawnCenter, spawnSize);
    }
}
