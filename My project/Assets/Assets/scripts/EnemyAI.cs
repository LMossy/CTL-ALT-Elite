using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public Transform target;               // drag in Inspector OR auto-find by tag
    public float repathInterval = 0.2f;

    NavMeshAgent agent;
    float nextRepathAt;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoRepath = true;
        agent.isStopped = false;
    }

    void Start()
    {
        // Snap onto NavMesh if not placed exactly on blue
        if (!agent.isOnNavMesh &&
            NavMesh.SamplePosition(transform.position, out var hit, 3f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        // Auto-find the player if not already assigned
        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player") ??
                    GameObject.FindGameObjectWithTag("player");  // supports both
            if (p) target = p.transform;
        }

        if (!target)
            Debug.LogError("[EnemyAI] Could not find player! Make sure your Player is tagged 'Player'.");
    }

    void Update()
    {
        if (!target || !agent.isOnNavMesh) return;

        if (Time.time >= nextRepathAt)
        {
            // Sample player position to ensure it’s on the NavMesh
            Vector3 dest = target.position;
            if (NavMesh.SamplePosition(dest, out var hit, 2f, NavMesh.AllAreas))
                dest = hit.position;

            agent.isStopped = false;
            agent.SetDestination(dest);
            nextRepathAt = Time.time + repathInterval;
        }
    }
}
