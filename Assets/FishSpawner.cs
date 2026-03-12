using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public GameObject fishPrefab;
    public int fishCount = 10;
    public Transform player;
    public float spawnDistance = 2f; // in front of player
    public float horizontalSpread = 5f;
    public float verticalSpread = 3f;

    void Start()
    {
        for (int i = 0; i < fishCount; i++)
        {
            // Position in front of player, with some random offset
            Vector3 randomOffset = new Vector3(
                Random.Range(-horizontalSpread, horizontalSpread),
                Random.Range(-verticalSpread, verticalSpread),
                Random.Range(0f, spawnDistance)
            );

            Vector3 spawnPos = player.position + player.forward * spawnDistance + randomOffset;

            // Rotate fish to face roughly forward + random slight vertical angle
            float angleVariation = Random.Range(-15f, 15f); // tilt up/down slightly
            Quaternion rot = Quaternion.Euler(angleVariation, player.eulerAngles.y, 0);

            GameObject fish = Instantiate(fishPrefab, spawnPos, rot);

            FishFollower follow = fish.GetComponent<FishFollower>();
            if (follow != null)
                follow.player = player;
        }
    }
}