using UnityEngine;

public class FishFollow : MonoBehaviour
{
    public Transform player;
    public float speed = 3f;
    public float followDistance = 6f;

    Vector3 swimOffset;

    void Start()
    {
        swimOffset = Random.insideUnitSphere * 3f;
    }

    void Update()
    {
        if (player == null) return;

        Vector3 target = player.position + swimOffset;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > followDistance)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                speed * Time.deltaTime
            );

            transform.LookAt(target);
        }
    }
}