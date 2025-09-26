using UnityEngine;
using UnityEngine.AI;

public class FloorSpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Vector2 areaXZ = new(12, 12); // size of the local floor area
    public LayerMask floorMask;          // set to your floor colliders
    public float rayStartHeight = 8f;
    public float snapRadius = 0.5f;

    public void Spawn()
    {
        var local = new Vector2(Random.Range(-areaXZ.x / 2, areaXZ.x / 2),
                                Random.Range(-areaXZ.y / 2, areaXZ.y / 2));
        var start = transform.position + new Vector3(local.x, rayStartHeight, local.y);

        if (Physics.Raycast(start, Vector3.down, out var hit, rayStartHeight * 2f, floorMask))
        {
            var p = hit.point + Vector3.up * 0.05f;
            if (NavMesh.SamplePosition(p, out var navHit, snapRadius, NavMesh.AllAreas))
                Instantiate(enemyPrefab, navHit.position, Quaternion.identity);
        }
    }
}
