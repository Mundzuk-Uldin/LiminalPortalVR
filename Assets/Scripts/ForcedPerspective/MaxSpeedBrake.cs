using UnityEngine;

public class MaxSpeedBrake : MonoBehaviour
{
    [SerializeField] private CharacterController cc;
    [SerializeField] private float maxSpeed = 8f;

    private Vector3 lastSafePos;

    void Awake()
    {
        if (cc == null) cc = GetComponent<CharacterController>();
        lastSafePos = transform.position;
    }

    void LateUpdate()
    {
        // compute actual speed from displacement
        float speed = (transform.position - lastSafePos).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);

        if (speed > maxSpeed)
        {
            transform.position = lastSafePos;
            // also reset your controller’s internal move vector if you have one
        }
        else
        {
            lastSafePos = transform.position;
        }
    }
}
