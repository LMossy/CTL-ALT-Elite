using UnityEngine;

public class EnemySpawnAtPlayer : MonoBehaviour
{
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Spawn at player’s position (with a little offset so it’s not stacked directly)
            transform.position = player.transform.position + Vector3.back * 2f;
        }
    }
}
