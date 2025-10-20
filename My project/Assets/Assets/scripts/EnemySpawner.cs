using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BoxCollider))]
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab & Limits")]
    public GameObject enemyPrefab;
    public int startCount = 3;
    public int maxEnemies = 10;

    [Header("Timing")]
    public float spawnDelay = 5f;

    [Header("Positioning")]
    public float navmeshSnapRadius = 1.5f;  // how far to search for a nearby NavMesh point
    public float minDistanceFromPlayer = 8f; // don't spawn right on top of the player

    BoxCollider area;
    float nextTime;
    Transform player;

    void Awake()
    {
        area = GetComponent<BoxCollider>();
        area.isTrigger = true; // safety: never block anything
        // Try to find the player by tag (supports both cases)
        var p = GameObject.FindGameObjectWithTag("Player") ??
                GameObject.FindGameObjectWithTag("player");
        if (p) player = p.transform;
    }

    void Start()
    {
        // Spawn initial batch
        for (int i = 0; i < startCount; i++) TrySpawn();
        nextTime = Time.time + spawnDelay;
    }

    void Update()
    {
        if (Time.time < nextTime) return;

        int count = CountAlive();
        if (count < maxEnemies) TrySpawn();

        nextTime = Time.time + spawnDelay;
    }

    int CountAlive()
    {
        // Cheapest reliable way: tag your enemy prefab as "Enemy"
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }

    bool TrySpawn()
    {
        if (!enemyPrefab) { Debug.LogWarning("[Spawner] No enemyPrefab assigned."); return false; }

        // Try multiple random points to find a valid NavMesh location
        for (int i = 0; i < 30; i++)
        {
            Vector3 world = RandomPointInBox(area);
            if (player && Vector3.Distance(world, player.position) < minDistanceFromPlayer)
                continue; // too close to player, pick another point

            if (NavMesh.SamplePosition(world, out var hit, navmeshSnapRadius, NavMesh.AllAreas))
            {
                var enemy = Instantiate(enemyPrefab, hit.position, Quaternion.identity);
                // Make sure the tag is correct for CountAlive()
                enemy.tag = "Enemy";
                return true;
            }
        }

        // If we get here, the box might not cover blue areas; increase size or snap radius
        // (or adjust Min Distance)
        // Debug.LogWarning("[Spawner] Failed to find NavMesh point—expand box or increase navmeshSnapRadius.");
        return false;
    }

    Vector3 RandomPointInBox(BoxCollider box)
    {
        // pick a random point within the collider volume, in world space
        Vector3 local = new Vector3(
            Random.Range(-box.size.x * 0.5f, box.size.x * 0.5f),
            Random.Range(-box.size.y * 0.5f, box.size.y * 0.5f),
            Random.Range(-box.size.z * 0.5f, box.size.z * 0.5f)
        );
        return box.transform.TransformPoint(box.center + local);
    }

    void OnDrawGizmosSelected()
    {
        // visualize the spawn region
        var col = GetComponent<BoxCollider>();
        if (!col) return;
        Gizmos.color = new Color(0f, 1f, 0f, 0.18f);
        Matrix4x4 m = Matrix4x4.TRS(transform.TransformPoint(col.center), transform.rotation, transform.lossyScale);
        Gizmos.matrix = m;
        Gizmos.DrawCube(Vector3.zero, col.size);
        Gizmos.color = new Color(0f, 0.7f, 0f, 0.9f);
        Gizmos.DrawWireCube(Vector3.zero, col.size);
    }
}
