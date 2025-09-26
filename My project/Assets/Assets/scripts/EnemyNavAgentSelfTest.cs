using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavAgentSelfTest : MonoBehaviour
{
    [Header("Chase")]
    [SerializeField] float repathInterval = 0.25f;
    [SerializeField] string playerTag = "Player";

    NavMeshAgent agent;
    Transform player;
    float nextRepath;
    NavMeshPath debugPath;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        debugPath = new NavMeshPath();

        // Good defaults for sprites in a 3D world
        agent.updateRotation = false;   // don't spin the sprite
        agent.updateUpAxis = false;     // keep it upright (optional)
        agent.autoBraking = true;
        agent.autoRepath = true;

        // Hard stop to “not on navmesh” issues: warp to nearest triangle
        if (NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            Debug.Log($"[Enemy] Warped to NavMesh at {hit.position}");
        }
        else
        {
            Debug.LogError($"[Enemy] No NavMesh within 5m of {transform.position}. Bake or move spawn.");
        }
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p) player = p.transform;

        if (!player)
            Debug.LogError("[Enemy] No Player found (tag 'Player').");
    }

    void Update()
    {
        // Live status every second
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[Enemy] isOnNavMesh={agent.isOnNavMesh}, hasPath={agent.hasPath}, " +
                      $"status={agent.pathStatus}, remaining={agent.remainingDistance:0.00}");
        }

        if (!player || !agent.isOnNavMesh) return;

        if (Time.time >= nextRepath)
        {
            agent.SetDestination(player.position);
            nextRepath = Time.time + repathInterval;

            // Also compute a debug path we can draw
            if (!NavMesh.CalculatePath(agent.transform.position, player.position, NavMesh.AllAreas, debugPath))
                Debug.LogWarning("[Enemy] CalculatePath failed.");
        }
    }

    void OnDrawGizmos()
    {
        // Big magenta dot at enemy position
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position + Vector3.up * 0.25f, 0.2f);

        // Draw the current path as a green line
        if (debugPath != null && debugPath.corners != null && debugPath.corners.Length > 1)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < debugPath.corners.Length - 1; i++)
                Gizmos.DrawLine(debugPath.corners[i] + Vector3.up * 0.05f,
                                debugPath.corners[i + 1] + Vector3.up * 0.05f);
        }
    }
}
