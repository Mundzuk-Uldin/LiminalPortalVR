using UnityEngine;

public class HoldableV2 : MonoBehaviour
{
    [SerializeField] private Transform heldTransform;
    [SerializeField] private Rigidbody heldRigidbody;
    [SerializeField] private Collider[] heldColliders;

    [SerializeField] private bool allowForcedPerspective = true;
    public bool AllowForcedPerspective => allowForcedPerspective;

    public Collider[] HeldColliders => heldColliders;   

    public Transform HeldTransform => heldTransform;
    public Rigidbody HeldRigidbody => heldRigidbody;

    public Vector3 Position => heldTransform.position;
    public Quaternion Rotation => heldTransform.rotation;

    public Vector3 LocalScale => heldTransform.localScale;
    private bool isBound;


    public void SetPosition(Vector3 position)
    {
        heldTransform.position = position;
    }

    public void Bind()
    {
        if (isBound) return;

        if (heldTransform == null)
            heldTransform = transform;

        if (heldRigidbody == null)
            heldRigidbody = GetComponentInChildren<Rigidbody>();

        if (heldColliders == null || heldColliders.Length == 0)
            heldColliders = GetComponentsInChildren<Collider>();

        isBound = true;
    }
    public void SetRotation(Quaternion rotation)
    {
        heldTransform.rotation = rotation;
    }

    public void SetPose(Vector3 position, Quaternion rotation)
    {
        heldTransform.SetPositionAndRotation(position, rotation);
    }

    

    public void SetLocalScale(Vector3 scale)
    {
        heldTransform.localScale = scale;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
}
