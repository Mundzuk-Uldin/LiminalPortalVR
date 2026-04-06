using UnityEngine;

public class HoldMotorV2 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float maxGrabDistance = 5f;
    [SerializeField] private LayerMask grabbableMask;

    [SerializeField] private CharacterController characterController;
    [SerializeField] private Portals.RigidbodyCharacterController RigidbodyCharacterController;

    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("Layers")]
    // [SerializeField] private string grabbableLayerName = "Grabbable";
    // [SerializeField] private string heldLayerName = "HeldObject";

    [Header("Player Collision (explicit)")]
    [Tooltip("These colliders represent the player (capsule/body/hands/etc). HoldMotor will ignore collisions against held object colliders while holding.")]
    [SerializeField] private Collider[] playerColliders;

    [Header("Hold Behaviour")]
    [Tooltip("If true, the held object can be scaled Superliminal-style, but ONLY if the Holdable opts-in (AllowScaling).")]
    // [SerializeField] private bool enableScaling;
    [SerializeField] private bool forcedPerspectiveEnabled = true;

    // [SerializeField] private float backstep = 0.02f;
    // [SerializeField] private int maxBacksteps = 120;
    [SerializeField] private float minDistance = 0.25f; // prevents pulling behind/into camera

    [SerializeField] private float wallClearance = 0.05f; // 5 cm leeway


    // [SerializeField] private float extraPlayerPadding = 0.05f;
    private int heldLayer;

    private float boundsBaseScale;


    [SerializeField] private float maxRayDistance = 100;
    // [SerializeField] private bool allowForcedPerspective;

    private HoldableV2 current;

    public bool IsHolding => current != null;

    private Vector3 grabbedLocalPoint;
    private float holdDistance;
    private Quaternion rotationOffset;

    private float initialHoldDistance;
    private Vector3 initialLocalScale;

    private Vector3 localBoundsCenter;
    private Vector3 localBoundsExtents;


    private int overlapMask;

    void Start()
    {

        grabbableMask = LayerMask.GetMask("Grabbable");
        int playerLayer = LayerMask.NameToLayer("Player");
        heldLayer   = LayerMask.NameToLayer("HeldObject");

        overlapMask = ~( (1 << playerLayer) | (1 << heldLayer) );
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        // If the held object was destroyed while holding, drop state safely.

        if (!IsHolding) return;

        if (current == null)
        {
            
            CleanupHold();
            return;
        }


        KeepHold(); /// keeps the hold!!!
    }

    public Collider[] GetPlayerColliders()
    {
        return GetComponentsInChildren<Collider>();
    }

    private Vector3 GetRayOrigin()
    {
        // PC implementation (for now)
        return playerCamera.transform.position;
    }

    private Vector3 GetRayDirection()
    {
        // PC implementation (for now)
        return playerCamera.transform.forward;
    }
    private Quaternion GetRayRotation()
    {
        // PC implementation (for now)
        return playerCamera.transform.rotation;
    }


    public void TryStartHold()
    {
        if (IsHolding) return;
        if (playerCamera == null) return;

        Ray ray = new Ray(GetRayOrigin(), GetRayDirection());

        // print("test");
        if (!Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance, grabbableMask, triggerInteraction))
            return;

        // Must be on Grabbable layer AND have HoldableV2
        if (hit.collider == null) return;


        HoldableV2 holdable = hit.collider.GetComponentInParent<HoldableV2>();
        if (holdable == null) return;
        // print("test");
        StartHold(holdable, hit.point);
    }

    public void StartHold(HoldableV2 holdable, Vector3 hitPoint)
    {
        if (holdable == null) return;
        if (IsHolding) return;

        current = holdable;
        holdable.Bind();
        current.SetLayerHeld(heldLayer);

        grabbedLocalPoint = holdable.HeldTransform.InverseTransformPoint(hitPoint);


        rotationOffset =
            Quaternion.Inverse(GetRayRotation()) *
            holdable.HeldTransform.rotation;

        // After grabbedLocalPoint and rotationOffset are set:

        Quaternion desiredRotationNow = GetRayRotation() * rotationOffset;


        // World offset from pivot to grabbed point (at current rotation)
        Vector3 worldGrabOffsetNow = desiredRotationNow * grabbedLocalPoint;

        // Current world position of the grabbed point
        Vector3 grabbedPointWorldNow = current.HeldTransform.position + worldGrabOffsetNow;

        // Project that grabbed point onto the ray to get the hold distance that causes NO snap
        Ray rayNow = new Ray(GetRayOrigin(), GetRayDirection());
        holdDistance = Vector3.Dot(grabbedPointWorldNow - rayNow.origin, rayNow.direction);

        initialHoldDistance =  holdDistance; //for test


        if (initialHoldDistance < current.minScale) initialHoldDistance = current.minScale;

        initialLocalScale = current.LocalScale; // your HoldableV2 getter

        Collider col = current.HeldColliders[0];

        if (col == null)
        {
            Debug.LogError("Held object has no collider.");
            return;
        }

        // CASE 1: BoxCollider (best case)
        if (col is BoxCollider box)
        {
            localBoundsCenter = box.center;
            localBoundsExtents = box.size * 0.5f;
        }
        // CASE 2: Other colliders (Sphere, Capsule, etc.)
        else
        {
            // Convert world bounds → local space approximation
            Bounds bounds = col.bounds;

            // Center in local space
            localBoundsCenter = current.transform.InverseTransformPoint(bounds.center);

            // Extents (scale-aware approximation)
            Vector3 worldExtents = bounds.extents;

            // Convert extents to local space (lossy but works)
            Vector3 localExtents = current.transform.InverseTransformVector(worldExtents);

            localBoundsExtents = new Vector3(
                Mathf.Abs(localExtents.x),
                Mathf.Abs(localExtents.y),
                Mathf.Abs(localExtents.z)
            );

            Debug.LogWarning($"Using approximate bounds for collider type: {col.GetType().Name}");
        }
        /*
        ///the change:
        // Use first BoxCollider as source of true local OBB data
        BoxCollider box = current.HeldColliders[0] as BoxCollider;

        if (box == null)
        {
            Debug.LogError("Held object does not have a BoxCollider as first collider.");
            return;
        }

        // True local center (already in local space)
        localBoundsCenter = box.center;

        // True local half extents
        localBoundsExtents = box.size * 0.5f;
        //*/

        // Store base scale
        boundsBaseScale = current.LocalScale.x;

        current.DisablePlayerCollision(GetPlayerColliders());
        current.DisablePhysicsForHold();

        var t = current.HeldTransform;
        Debug.Log($"[HoldStart] localScale={t.localScale} lossyScale={t.lossyScale}");
    }

    private void KeepHold()
    {
        if (!IsHolding) return;

        // 1) Rebuild the ray every frame
        Ray ray = new Ray(GetRayOrigin(), GetRayDirection());
        
        // 2) Rebuild desired rotation (camera-relative)
        Quaternion desiredRotation = GetRayRotation() * rotationOffset;

        if(ShouldForcePerspectiveScale()){
            // Debug.Log($"[before apply] localpos={current.Position}");

            ApplyForcedPerspective(ray, desiredRotation);
        } else
        {
            // 3) Compute desired grab point along the ray
            Vector3 desiredGrabPoint =
                ray.origin + ray.direction * holdDistance;

            float s = current.LocalScale.x; // assume uniform scale
            Vector3 worldGrabOffset = desiredRotation * (grabbedLocalPoint * s);
            Vector3 desiredPosition = desiredGrabPoint - worldGrabOffset;

            current.StepPose_Tight(desiredPosition, desiredRotation);

        }

        
    }

    private void ApplyForcedPerspective(Ray ray, Quaternion desiredRotation)
    {

        float hi = GetWallHitDistance(ray);

        if (hi < minDistance) hi = minDistance;

        float lo = minDistance;

        float playerMin = GetPlayerMinHoldDistance();
        lo = Mathf.Max(lo, playerMin);
        hi = Mathf.Max(hi, playerMin);

        if (hi <= lo + 0.001f)
            hi = lo;

        // --- helper local function (keeps this readable) ---
        bool TestAt(float dist, out Vector3 pos, out Vector3 scale)

        {
            scale = GetDesiredScale(dist);
            float absScale = scale.x;                    // ABS scale used for pose glue

            Vector3 grabPoint = ray.origin + ray.direction * dist;

            // Glue offset MUST use absolute scale (not relative)
            Vector3 offset = desiredRotation * (grabbedLocalPoint * absScale);

            pos = grabPoint - offset;
            return !IsOverlapping(pos, desiredRotation, absScale);
        }

        // 1) ensure lo is valid
        if (!TestAt(lo, out Vector3 loPos,  out Vector3 loScale))
            return;

        // float best = lo;
        Vector3 bestPos = loPos;
        Vector3 bestScale = loScale;

        for (int i = 0; i < 12; i++) // 12 iterations is plenty
        {
            float mid = (lo + hi) * 0.5f;



            if (TestAt(mid, out Vector3 midPos, out Vector3 midScale))
            {
                // best = mid;
                bestPos = midPos;
                bestScale = midScale;
                lo = mid;     // valid → try closer to wall
            }
            else
            {
                
                hi = mid;     // invalid → back off toward player
            }
        }

        // 3) apply final pose + scale
        current.StepPose_Tight(bestPos, desiredRotation);
        current.SetLocalScale(bestScale);
    }
    //*/

    private float GetWallHitDistance(Ray ray)
    {
        if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                maxRayDistance,
                overlapMask,
                QueryTriggerInteraction.Ignore))
        {
            return hit.distance;
        }

        return maxRayDistance;
    }
    private bool IsOverlapping(Vector3 testPosition,Quaternion testRotation,float scaleFactor)
    {
        // Compute where the bounds center would be at the TEST pose
        Vector3 testCenter =
            testPosition + testRotation * (localBoundsCenter * scaleFactor);

        // Compute world half extents
        Vector3 halfExtents = localBoundsExtents * scaleFactor;

        Collider[] hits = Physics.OverlapBox(
            testCenter,
            halfExtents,
            testRotation,
            overlapMask,
            QueryTriggerInteraction.Ignore
        );

        
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null) continue;

            // Ignore self colliders
            if (hits[i].transform.IsChildOf(current.HeldTransform))
                continue;

            return true;
        }

        return false;
    }


    private Bounds GetHeldWorldBounds()
    {
        Collider[] cols = current.HeldColliders;

        Bounds b = cols[0].bounds;
        for (int i = 1; i < cols.Length; i++)
            b.Encapsulate(cols[i].bounds);

        return b;
    }



    private void CleanupHold()
    {
        current = null;
    }
    public void EndHold()
    {
        var t = current.HeldTransform;
        // Debug.Log($"[HoldEnd] localScale={t.localScale} lossyScale={t.lossyScale}");
        // Debug.Log($"[HoldEnd] localpos={current.Position}");
        if (current == null) return;
        current.RestoreLayer();

        current.EnablePhysicsAfterHold();
        current.EnablePlayerCollision(GetPlayerColliders());
        CleanupHold();
    }

    private bool ShouldForcePerspectiveScale()
    {
        return forcedPerspectiveEnabled && current != null && current.AllowForcedPerspective;
    }
    private Vector3 GetDesiredScale(float currentDistance)
    {
        float t = currentDistance / initialHoldDistance;
        return initialLocalScale * t;
    }   
    

    private float GetPlayerMinHoldDistance()
    {
        float playerRadius = 0.35f;
        float skin = 0.02f;
        if (characterController != null)
        {
            playerRadius = characterController.radius;
            skin = characterController.skinWidth; 
        }
        else if (RigidbodyCharacterController != null)
        {
            playerRadius = RigidbodyCharacterController.PlayerRadius;
            skin = RigidbodyCharacterController.SkinWidth; 
        }

        float objRadius = current.GetCollisionRadius();

        return playerRadius + skin + objRadius; 
    }


    private void SetIgnorePlayerCollisions(HoldableV2 holdable, bool ignore)
    {

    }
}

