using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SnapToGroundAtStart : MonoBehaviour
{
    void Start()
    {
        var agent = GetComponent<NavMeshAgent>();
        agent.baseOffset = 0f;

        // Drop a raycast straight down to find the ground
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, 5f))
        {
            agent.Warp(hit.point); // move agent capsule to the floor
        }
    }
}
