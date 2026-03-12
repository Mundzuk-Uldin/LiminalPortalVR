using UnityEngine;

public class FishPathFollower : MonoBehaviour
{
    [HideInInspector] public Transform[] pathPoints;  // assigned by spawner
    [HideInInspector] public Transform player;        // assigned by spawner

    [Header("Movement Settings")]
    public float speed = 3f;
    public float stopDistance = 5f;    // distance from player to pause movement
    public float waypointThreshold = 0.5f; // how close to waypoint to consider "reached"

    [Header("Bobbing Settings")]
    public float bobAmplitude = 0.5f;
    public float bobFrequency = 1f;

    private int currentPoint = 0;
    private float baseY;               // Y position for bobbing
    private Vector3 velocity;          // optional for smooth movement

    void Start()
    {
        baseY = transform.position.y;
    }

    void Update()
    {
        if (pathPoints == null || pathPoints.Length == 0 || player == null)
            return;

        float playerDistance = Vector3.Distance(player.position, transform.position);

        // Pause movement if player is too far
        if (playerDistance > stopDistance)
        {
            BobInPlace();
            return;
        }

        MoveAlongPath();
        BobInPlace(0.1f);
    }

    private void MoveAlongPath()
    {
        Vector3 target = pathPoints[currentPoint].position;

        // Calculate horizontal direction only (preserve bobbing Y)
        Vector3 horizontalTarget = new Vector3(target.x, transform.position.y, target.z);
        Vector3 direction = (horizontalTarget - transform.position).normalized;

        // Move fish
        transform.position += direction * speed * Time.deltaTime;

        // Rotate toward waypoint
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f);
        }

        // Check if waypoint reached
        if ((new Vector3(transform.position.x, 0, transform.position.z) - 
             new Vector3(target.x, 0, target.z)).sqrMagnitude < waypointThreshold * waypointThreshold)
        {
            currentPoint = (currentPoint + 1) % pathPoints.Length;
        }
    }

    private void BobInPlace(float amplitudeOverride = -1f)
    {
        float amplitude = amplitudeOverride > 0 ? amplitudeOverride : bobAmplitude;
        Vector3 pos = transform.position;
        pos.y = baseY + Mathf.Sin(Time.time * bobFrequency) * amplitude;
        transform.position = pos;
    }
}