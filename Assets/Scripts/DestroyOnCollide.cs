using UnityEngine;

public class DestroyOnCollide : MonoBehaviour
{
    public GameObject objectToDestroy;
    public SpawnPortalOnClick portalSpawner;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<SpawnPortalOnClick>()!.DespawnPortals();
            Destroy(objectToDestroy);
        }
    }

    
}
