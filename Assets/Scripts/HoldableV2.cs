using UnityEngine;

public class HoldableV2 : MonoBehaviour
{
    [SerializeField] private Transform heldTransform;
    [SerializeField] private Rigidbody heldRigidbody;
    [SerializeField] private Collider[] heldColliders;

    [SerializeField] private bool allowForcedPerspective = true;

        [Header("Hold Limits")]
    // public float minDistance = 0.5f;
    // public float maxDistance = 20f;
    public float minScale = 0.1f;
    public float maxScale = 10f;
    public float surfacePadding = 0.05f;

    private int[] originalLayers;
    private Transform[] cachedTransforms;
    //     // Tune these in inspector or constants — your call.
    // [SerializeField] float snapDistance = 0.02f;   // 2 cm: snap when close
    // [SerializeField] float maxSpeed = 25f;         // units/sec, increase for “more grabby”

    // [SerializeField] float snapAngle = 1.0f;          // degrees
    // [SerializeField] float maxDegPerSec = 720f;       // fast, feels “hand-locked”
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
        // old version
        // heldTransform.position = position;

        //new version
        heldRigidbody.MovePosition(position);
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
        Debug.Log($" starting bind:\nHeldTransform={heldTransform.name}, RBTransform={heldRigidbody.transform.name}, same={heldTransform==heldRigidbody.transform}");

    }
    public void SetRotation(Quaternion rotation)
    {
        ///old
        heldTransform.rotation = rotation;

        //new 
        heldRigidbody.MoveRotation(rotation);
    }

    public void SetPose(Vector3 position, Quaternion rotation)
    {
        heldTransform.SetPositionAndRotation(position, rotation);
    }



    public void SetLocalScale(Vector3 scale)
    {
        float s = Mathf.Clamp(scale.x, minScale, maxScale);
        heldTransform.localScale = Vector3.one * s;
    }

    public void DisablePhysicsForHold()
    {
        if (heldRigidbody == null) return;

        heldRigidbody.linearVelocity= Vector3.zero;
        heldRigidbody.angularVelocity = Vector3.zero;

        heldRigidbody.useGravity = false;
        heldRigidbody.isKinematic = true;

        heldRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        heldRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
    }

    public void EnablePhysicsAfterHold()
    {
        if (heldRigidbody == null) return;

        heldRigidbody.isKinematic = false;
        heldRigidbody.useGravity = true;

        heldRigidbody.interpolation = RigidbodyInterpolation.None;
        heldRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
    }

    public void StepPose_Tight(Vector3 targetPos, Quaternion targetRot)
    {
        StepPosition_Tight(targetPos);
        StepRotation_Tight(targetRot);
    }

    public void StepPosition_Tight(Vector3 targetPos)
    {

        const float snapDistance = 0.02f;   // 2 cm: snap when close
        const float maxSpeed = 300f;         // units/sec, increase for “more grabby” def 25f

        Vector3 p = heldTransform.position;
        Vector3 delta = targetPos - p;

        float dist = delta.magnitude;
        if (dist <= snapDistance || dist < 0.00001f)
        {
            heldTransform.position = targetPos;
            return;
        }

        float step = maxSpeed * Time.deltaTime;
        if (step >= dist)
        {
            heldTransform.position = targetPos;
            return;
        }

        heldRigidbody.MovePosition(p + (delta / dist) * step);
    }

    public void StepRotation_Tight(Quaternion targetRot)
    {

        const float snapAngle = 1.0f;          // degrees
        const float maxDegPerSec = 4000f;       // fast, feels “hand-locked” def 720f

        Quaternion r = heldTransform.rotation;
        float angle = Quaternion.Angle(r, targetRot);

        if (angle <= snapAngle)
        {
            heldTransform.rotation = targetRot;
            return;
        }

        float maxStep = maxDegPerSec * Time.deltaTime;
        if (angle <= snapAngle)
        {
            heldTransform.rotation = targetRot;
            return;
        }
        const float deadzoneDeg = 0.05f; // try 0.02–0.15
        if (angle <= deadzoneDeg)
        {
            heldTransform.rotation = targetRot;
            return;
        }
        heldTransform.rotation = Quaternion.RotateTowards(r, targetRot, maxStep);
    }

    public void DisablePlayerCollision(Collider[] playerColliders)
    {
        if (heldColliders == null || heldColliders.Length == 0)
            heldColliders = GetComponentsInChildren<Collider>();

        if (playerColliders == null || playerColliders.Length == 0)
            return; // player colliders must be provided once from outside

        for (int i = 0; i < heldColliders.Length; i++)
        {
            var hc = heldColliders[i];
            if (!hc) continue;

            for (int j = 0; j < playerColliders.Length; j++)
            {
                var pc = playerColliders[j];
                if (!pc) continue;

                Physics.IgnoreCollision(hc, pc, true);
            }
        }
    }

    public void EnablePlayerCollision(Collider[] playerColliders)
    {
        if (heldColliders == null || heldColliders.Length == 0)
            heldColliders = GetComponentsInChildren<Collider>();

        if (playerColliders == null || playerColliders.Length == 0)
            return;

        for (int i = 0; i < heldColliders.Length; i++)
        {
            var hc = heldColliders[i];
            if (!hc) continue;

            for (int j = 0; j < playerColliders.Length; j++)
            {
                var pc = playerColliders[j];
                if (!pc) continue;

                Physics.IgnoreCollision(hc, pc, false);
            }
        }
    }
 
    public float GetCollisionRadius()
    {
        Collider[] cols = GetComponentsInChildren<Collider>();
        if (cols == null || cols.Length == 0)
            return 0.1f;

        Bounds b = cols[0].bounds;
        for (int i = 1; i < cols.Length; i++)
            b.Encapsulate(cols[i].bounds);

        // radius = largest half-extent
        return Mathf.Max(b.extents.x, b.extents.y, b.extents.z);
    }



    public void SetLayerHeld(int heldLayer)
    {
        cachedTransforms = HeldTransform.GetComponentsInChildren<Transform>(true);
        originalLayers = new int[cachedTransforms.Length];

        for (int i = 0; i < cachedTransforms.Length; i++)
        {
            originalLayers[i] = cachedTransforms[i].gameObject.layer;
            cachedTransforms[i].gameObject.layer = heldLayer;
        }
    }

    public void RestoreLayer()
    {
        if (cachedTransforms == null || originalLayers == null) return;

        for (int i = 0; i < cachedTransforms.Length; i++)
        {
            if (cachedTransforms[i] != null)
                cachedTransforms[i].gameObject.layer = originalLayers[i];
        }

        cachedTransforms = null;
        originalLayers = null;
    }

    
}
