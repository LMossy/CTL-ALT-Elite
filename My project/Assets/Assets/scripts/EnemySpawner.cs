using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Setup")]
    public GameObject enemyPrefab;      // assign your enemy prefab
    public int numberOfEnemies = 1;     // how many to spawn

    [Header("Spawn Area")]
    public Vector3 center = Vector3.zero; // middle of spawn zone
    public Vector3 size = new Vector3(20, 1, 20); // width, height, depth of zone

    void Start()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        // Pick a random position inside the defined area
        Vector3 pos = center + new Vector3(
            Random.Range(-size.x / 2, size.x / 2),
            Random.Range(-size.y / 2, size.y / 2),
            Random.Range(-size.z / 2, size.z / 2)
        );

        Instantiate(enemyPrefab, pos, Quaternion.identity);
    }

    // Draws a wireframe box in the editor so you can see the spawn area
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, size);
    }
}
