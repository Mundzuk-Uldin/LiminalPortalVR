using UnityEngine;

public class Holdable : MonoBehaviour
{
    [Header("Hold Limits")]
    public float minDistance = 0.5f;
    public float maxDistance = 20f;
    public float minScale = 0.1f;
    public float maxScale = 10f;
    public float surfacePadding = 0.05f;

    // [Header("References")]
    // public Rigidbody[] rigidbodies;

    [Header("Physics Refs (your child RBs)")]
    [SerializeField] private Rigidbody primaryRb;

    [SerializeField] private Collider[] heldColliders;
    [SerializeField] private Collider[] playerColliders;
    [SerializeField] private Transform playerRoot; // assign to your Player capsule


    [Header("Follow Tuning")]
    [Tooltip("Higher = follows target more aggressively")]
    public float followSpeed = 40f;

    private Vector3 targetPos;
    private Vector3 targetScale;
    private bool hasTarget;

    // Remember original RB settings to restore
    private bool origUseGravity;
    private bool origIsKinematic;
    private RigidbodyInterpolation origInterpolation;
    private CollisionDetectionMode origCollisionMode;
    private int originalLayer;

    // void Awake()
    // {
    //     originalLayer = gameObject.layer;

    //     if (rigidbodies == null || rigidbodies.Length == 0)
    //         rigidbodies = GetComponentsInChildren<Rigidbody>();
    // }

    void Awake()
    {
        originalLayer = gameObject.layer;

        if (primaryRb == null)
            primaryRb = GetComponentInChildren<Rigidbody>();

        if (primaryRb != null)
        {
            origUseGravity = primaryRb.useGravity;
            origIsKinematic = primaryRb.isKinematic;
            origInterpolation = primaryRb.interpolation;
            origCollisionMode = primaryRb.collisionDetectionMode;
        }

        if (heldColliders == null || heldColliders.Length == 0)
        heldColliders = GetComponentsInChildren<Collider>();

        // playerRoot should be set by HoldMotor/Grabber, but as fallback:
        // you can FindWithTag("Player") if you use a Player tag
        // if (playerColliders == null || playerColliders.Length == 0)
        // playerColliders = FindWithTag("Player").GetComponents<Collider>(); //GetComponentsInChildren<Collider>();
        // playerColliders = 
    }
    // public void OnGrab()
    // {
    //     originalLayer = gameObject.layer;
    //     gameObject.layer = LayerMask.NameToLayer("HeldObject");

    //     foreach (var rb in rigidbodies)
    //     {
    //         rb.isKinematic = true;
    //         rb.useGravity = false;
    //     }
    // }
    public void SetPlayerRoot(Transform player)
    {
        playerRoot = player;
        if (playerRoot != null)
            playerColliders = playerRoot.GetComponentsInChildren<Collider>();
    }

    private void SetIgnorePlayerCollisions(bool ignore)
    {
        if (playerColliders == null || playerColliders.Length == 0) return;

        foreach (var hc in heldColliders)
        foreach (var pc in playerColliders)
            if (hc != null && pc != null)
                Physics.IgnoreCollision(hc, pc, ignore);
    }
    public void OnGrab()
    {
        originalLayer = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("HeldObject");

        hasTarget = false;

        if (primaryRb != null)
        {
            // While held, we want collision-aware motion without explosions.
            primaryRb.useGravity = false;

            // We can keep it kinematic and move via MovePosition.
            primaryRb.isKinematic = true;

            // Helps smooth and reduces tunneling issues
            primaryRb.interpolation = RigidbodyInterpolation.Interpolate;
            primaryRb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }
        SetIgnorePlayerCollisions(true);
    }

    // public void OnRelease()
    // {
    //     gameObject.layer = originalLayer;

    //     foreach (var rb in rigidbodies)
    //     {
    //         rb.isKinematic = false;
    //         rb.useGravity = true;
    //     }
    // }
    public void OnRelease()
    {
        SetIgnorePlayerCollisions(false);

        gameObject.layer = originalLayer;
        hasTarget = false;

        if (primaryRb != null)
        {
            primaryRb.useGravity = origUseGravity;
            primaryRb.isKinematic = origIsKinematic;
            primaryRb.interpolation = origInterpolation;
            primaryRb.collisionDetectionMode = origCollisionMode;
        }
    }

    // Called by HoldMotor every frame
    public void SetHoldTarget(Vector3 desiredPos, Vector3 desiredScale)
    {
        targetPos = desiredPos;
        targetScale = desiredScale;
        hasTarget = true;
    }

    void FixedUpdate()
    {
        if (!hasTarget) return;

        // Scaling: do it here (or let HoldMotor do it; but keep it in ONE place).
        transform.localScale = targetScale;

        if (primaryRb == null)
        {
            // Fallback: if no RB found, do transform move (not ideal)
            transform.position = targetPos;
            return;
        }

        // Physics-safe movement: move toward target
        Vector3 current = primaryRb.position;

        // Smooth approach (frame-rate independent)
        float t = 1f - Mathf.Exp(-followSpeed * Time.fixedDeltaTime);
        Vector3 next = Vector3.Lerp(current, targetPos, t);

        primaryRb.MovePosition(next);
    }

    public float GetCollisionRadius()
    {
        Collider col = GetComponentInChildren<Collider>();
        if (col == null) return 0.1f;

        // Approximate radius using bounds
        Bounds b = col.bounds;
        return Mathf.Max(b.extents.x, b.extents.y, b.extents.z);
    }

}
