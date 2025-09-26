using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SpawnExactlyOnPlayerFloor : MonoBehaviour
{
    [SerializeField] string playerTag = "Player";
    [SerializeField] LayerMask groundMask;     // assign your floor/ground layers in Inspector
    [SerializeField] float rayUp = 2f;         // start ray slightly above player
    [SerializeField] float rayDown = 6f;       // how far to raycast down
    [SerializeField] float sampleRadius = 0.4f;// small so it won't jump to upstairs mesh

    void Start()
    {
        var agent = GetComponent<NavMeshAgent>();
        agent.baseOffset = 0f;
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        var pObj = GameObject.FindGameObjectWithTag(playerTag);
        if (!pObj) { Debug.LogError("No Player tagged 'Player' found."); return; }

        // 1) Raycast straight down from near the player to hit *this* floor
        Vector3 start = pObj.transform.position + Vector3.up * rayUp;
        if (Physics.Raycast(start, Vector3.down, out var hit, rayDown, groundMask))
        {
            // 2) Nudge up a hair to avoid z-fighting
            Vector3 pos = hit.point + Vector3.up * 0.02f;

            // 3) Snap to NavMesh using a SMALL radius so it can't snap to upstairs
            if (NavMesh.SamplePosition(pos, out var navHit, sampleRadius, NavMesh.AllAreas))
                agent.Warp(navHit.position);
            else
                agent.Warp(pos); // fall back to the floor point
        }
        else
        {
            // If no ground mask is set or ray misses, just warp to player's position
            if (NavMesh.SamplePosition(pObj.transform.position, out var navHit2, sampleRadius, NavMesh.AllAreas))
                agent.Warp(navHit2.position);
            else
                agent.Warp(pObj.transform.position);
        }
    }
}

