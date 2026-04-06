using UnityEngine;

public class ScaleBasedMass : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody childRigidbody;

    [Header("Mass Settings")]
    [SerializeField] private float baseMass = 1f; // Mass at scale (1,1,1)
    [SerializeField] private float massMultiplier = 1f; // Tuning factor

    [Header("Push Settings")]
    [SerializeField] private float immovableMassThreshold = 50f;
    [SerializeField] private bool makeImmovable = true;

    private void Awake()
    {
        if (childRigidbody == null)
        {
            childRigidbody = GetComponentInChildren<Rigidbody>();
        }
        
        if (childRigidbody == null)
        {
            childRigidbody = GetComponent<Rigidbody>();
        }
        ApplyMassFromScale();
    }

    private void ApplyMassFromScale()
    {
        Vector3 scale = transform.localScale;

        // Volume-based scaling (more realistic than linear)
        float scaleFactor = scale.x * scale.y * scale.z;

        float newMass = baseMass * scaleFactor * massMultiplier;

        childRigidbody.mass = newMass;

        // Handle immovable behavior
        if (makeImmovable && newMass >= immovableMassThreshold)
        {
            MakeImmovable();
        }
        else
        {
            MakeMovable();
        }
    }

    private void MakeImmovable()
    {
        // Option 1: Freeze movement completely
        childRigidbody.constraints = RigidbodyConstraints.FreezeAll;

        // Optional: Increase drag to absurd levels instead (less harsh)
        // childRigidbody.drag = 1000f;
        // childRigidbody.angularDrag = 1000f;
    }

    private void MakeMovable()
    {
        childRigidbody.constraints = RigidbodyConstraints.None;
    }
}