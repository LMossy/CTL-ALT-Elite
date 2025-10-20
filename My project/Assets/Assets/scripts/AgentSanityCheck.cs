// AgentSanityTest.cs
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentSanityTest : MonoBehaviour
{
    public string playerTag1 = "player";
    public string playerTag2 = "Player";
    public float repath = 0.2f;      // seconds
    public float snapRadius = 3f;    // how far we search to snap to navmesh
    NavMeshAgent agent;
    Transform target;
    float nextSet;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoRepath = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        // sensible defaults
        if (agent.radius < 0.1f) agent.radius = 0.3f;
        if (agent.height < 0.5f) agent.height = 1.6f;
        agent.baseOffset = 0f;
    }

    void Start()
    {
        // 1) Make 100% sure we are on the NavMesh
        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, snapRadius, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                Debug.Log("[AgentSanityTest] Warped onto NavMesh at " + hit.position);
            }
            else
            {
                Debug.LogError("[AgentSanityTest] No NavMesh near enemy! Move enemy onto blue or rebake.");
            }
        }

        // 2) Find player
        var p = GameObject.FindGameObjectWithTag(playerTag1) ??
                GameObject.FindGameObjectWithTag(playerTag2);
        if (p) target = p.transform;
        else Debug.LogError("[AgentSanityTest] Could not find player with tags 'player' or 'Player'.");
    }

    void Update()
    {
        if (!agent) return;

        // live diagnostics (once per 0.5s)
        if (Time.frameCount % 30 == 0)
        {
            Debug.Log($"[AgentSanityTest] OnNav:{agent.isOnNavMesh} HasPath:{agent.hasPath} " +
                      $"Status:{agent.pathStatus} Vel:{agent.velocity.magnitude:0.00} " +
                      $"Rem:{agent.remainingDistance:0.00} AgentTypeID:{agent.agentTypeID}");
        }

        if (!target || !agent.isOnNavMesh) return;

        if (Time.time >= nextSet)
        {
            agent.SetDestination(target.position);
            nextSet = Time.time + repath;
        }

        // draw a line so we see where it's trying to go
        Debug.DrawLine(transform.position, target.position, Color.yellow);
    }
}
