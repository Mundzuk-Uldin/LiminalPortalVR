using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public GameObject fishPrefab;
    public int fishCount = 10;
    public Transform player;
    public Transform[] pathPoints;  // assign your path points in the inspector

    public float spawnDistance = 2f;    // in front of spawner
    public float horizontalSpread = 5f; // X/Z randomness
    public float verticalSpread = 2f;   // Y randomness

    void Start()
    {
        if (fishPrefab == null || player == null || pathPoints.Length == 0)
        {
            Debug.LogError("FishSpawner missing references!");
            return;
        }

        for (int i = 0; i < fishCount; i++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-horizontalSpread, horizontalSpread),
                Random.Range(-verticalSpread, verticalSpread),
                Random.Range(0f, spawnDistance)
            );

            Vector3 spawnPos = transform.position + randomOffset;

            GameObject fish = Instantiate(fishPrefab, spawnPos, Quaternion.identity);

            FishPathFollower follower = fish.GetComponent<FishPathFollower>();
            if (follower != null)
            {
                follower.player = player;
                follower.pathPoints = pathPoints;
            }

            // Optional: make fish children of spawner to keep hierarchy clean
            fish.transform.parent = transform;
        }
    }
}