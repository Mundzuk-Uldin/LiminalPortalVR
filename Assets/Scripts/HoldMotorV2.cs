using UnityEngine;

public class HoldMotorV2 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float maxGrabDistance = 5f;
    [SerializeField] private LayerMask grabbableMask;

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

    [SerializeField] private float backstep = 0.02f;
    [SerializeField] private int maxBacksteps = 120;
    [SerializeField] private float minDistance = 0.25f; // prevents pulling behind/into camera

    private float maxRayDistance = 500;
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
        int heldLayer   = LayerMask.NameToLayer("HeldObject");

        overlapMask = ~( (1 << playerLayer) | (1 << heldLayer) );
    }

    // Update is called once per frame
    private void Update()
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
        // print("starting hold");

        Ray ray = new Ray(GetRayOrigin(), GetRayDirection());


        // print("player pos:" + playerCamera.transform.position.ToString());
        if (!Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance, grabbableMask, triggerInteraction))
            return;

        // Must be on Grabbable layer AND have HoldableV2
        if (hit.collider == null) return;


        HoldableV2 holdable = hit.collider.GetComponentInParent<HoldableV2>();
        if (holdable == null) return;

        StartHold(holdable, hit.point);
    }

    public void StartHold(HoldableV2 holdable, Vector3 hitPoint)
    {
        if (holdable == null) return;
        if (IsHolding) return;
        print("does it hit anything? " + holdable.name.ToString());

        current = holdable;
        holdable.Bind();

        grabbedLocalPoint = holdable.HeldTransform.InverseTransformPoint(hitPoint);
        holdDistance = Vector3.Distance(
            GetRayOrigin(),
            hitPoint
        );
        rotationOffset =
            Quaternion.Inverse(GetRayRotation()) *
            holdable.HeldTransform.rotation;


        initialHoldDistance = holdDistance;
        if (initialHoldDistance < 0.0001f) initialHoldDistance = 0.0001f;

        initialLocalScale = current.LocalScale; // your HoldableV2 getter

        // Combine all collider bounds in WORLD space
        Collider[] cols = current.HeldColliders;
        Bounds worldBounds = cols[0].bounds;
        for (int i = 1; i < cols.Length; i++)
            worldBounds.Encapsulate(cols[i].bounds);

        // Convert bounds center into LOCAL space
        localBoundsCenter =
            current.HeldTransform.InverseTransformPoint(worldBounds.center);

        // Convert extents into LOCAL-ish space (uniform scale assumption)
        float s = current.LocalScale.x;
        if (s < 0.0001f) s = 0.0001f;

        localBoundsExtents = worldBounds.extents / s;

        print("does it hit anything? " + holdable.name.ToString());
        print("are we holding? " + IsHolding.ToString());
        // currentOriginalLayer = holdable.gameObject.layer;

        

    }

    private void KeepHold()
    {
        if (!IsHolding) return;

        // 1) Rebuild the ray every frame
        Ray ray = new Ray(GetRayOrigin(), GetRayDirection());
        
        // 2) Rebuild desired rotation (camera-relative)
        Quaternion desiredRotation = GetRayRotation() * rotationOffset;

        if(ShouldForcePerspectiveScale()){
            
            ApplyForcedPerspective(ray, desiredRotation);
        } else
        {
            // 3) Compute desired grab point along the ray
            Vector3 desiredGrabPoint =
                ray.origin + ray.direction * holdDistance;

            // 4) Convert local grab point back into world space
            Vector3 worldGrabOffset =
                desiredRotation * grabbedLocalPoint;

            // 5) Compute final object position
            Vector3 desiredPosition =
                desiredGrabPoint - worldGrabOffset;

            // 6) Apply pose
            current.SetPosition(desiredPosition);
            current.SetRotation(desiredRotation);
        }

        
    }

    private void ApplyForcedPerspective(Ray ray, Quaternion desiredRotation)
    {
        // 1) Find the front-most solid point in front of the player
        // float targetDistance = GetWallHitDistance(ray);
        // if (targetDistance < minDistance) targetDistance = minDistance;

        // float testDistance = targetDistance;

        // print("are we scalin? " + testDistance);

        float hi = GetWallHitDistance(ray);

        if (hi < minDistance) hi = minDistance;

        print("are we scalin? " + hi);

        float lo = minDistance;

        // First ensure lo is actually free. If not, don't move this frame.
        Vector3 loScale = GetDesiredScale(lo);
        float loFactor = loScale.x / initialLocalScale.x;

        Vector3 loGrabPoint = ray.origin + ray.direction * lo;
        Vector3 loOffset = desiredRotation * (grabbedLocalPoint * loFactor);
        Vector3 loPos = loGrabPoint - loOffset;
        print("step 1");

        if (IsOverlapping(loPos, desiredRotation, loFactor))
            return; // nowhere valid to place
        
        print("step 2");
        // Binary search for closest valid position
        float best = lo;

        for (int i = 0; i < 12; i++) // 12 iterations is plenty
        {
            float mid = (lo + hi) * 0.5f;

            Vector3 midScale = GetDesiredScale(mid);
            float midFactor = midScale.x / initialLocalScale.x;

            Vector3 midGrabPoint = ray.origin + ray.direction * mid;
            Vector3 midOffset = desiredRotation * (grabbedLocalPoint * midFactor);
            Vector3 midPos = midGrabPoint - midOffset;

            if (IsOverlapping(midPos, desiredRotation, midFactor))
            {
                // too close; back off
                hi = mid;
            }
            else
            {
                // valid; try closer
                best = mid;
                lo = mid;
            }
        }

        print("step 3");
        Vector3 finalScale = GetDesiredScale(best);
        float finalFactor = finalScale.x / initialLocalScale.x;

        Vector3 finalGrabPoint = ray.origin + ray.direction * best;
        Vector3 finalOffset = desiredRotation * (grabbedLocalPoint * finalFactor);
        Vector3 finalPos = finalGrabPoint - finalOffset;

        print("did we even get here?");
        current.SetLocalScale(finalScale);
        current.SetPosition(finalPos);
        current.SetRotation(desiredRotation);
        /*
        for (int i = 0; i < maxBacksteps; i++)
        {
            // 2) Determine scale at this distance (uniform)
            Vector3 desiredScale = GetDesiredScale(testDistance);

            // uniform scale factor (assumes your initialLocalScale is uniform)
            float scaleFactor = desiredScale.x / initialLocalScale.x;

            // 3) Where should the grabbed point be on the ray at this distance?
            Vector3 desiredGrabPoint = ray.origin + ray.direction * testDistance;

            // 4) Compute position that keeps the grabbed point glued (rotation + scale)
            Vector3 worldGrabOffset = desiredRotation * (grabbedLocalPoint * scaleFactor);
            Vector3 desiredPosition = desiredGrabPoint - worldGrabOffset;

            // 5) Collision safety: if overlapping, step back and try again
            if (IsOverlapping(desiredPosition, desiredRotation, scaleFactor))
            {
                testDistance -= backstep;
                if (testDistance < minDistance) break;
                continue;
            }

            // 6) Apply scale + pose (this path owns both)
            current.SetLocalScale(desiredScale);
            current.SetPosition(desiredPosition);
            current.SetRotation(desiredRotation);
            return;
        }
        //*/

        // fallback: couldn't find a valid non-overlapping placement
        // do nothing (keeps last frame pose) or clamp to minDistance if you prefer
    }

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
            Debug.Log("Overlap hit: " + hits[i].name + " layer=" + LayerMask.LayerToName(hits[i].gameObject.layer));
            if (hits[i] == null) continue;

            // Ignore self colliders
            if (hits[i].transform.IsChildOf(current.HeldTransform))
                continue;

            return true;
        }

        return false;
    }


    /* old IsOverlapping
    private bool IsOverlapping(Vector3 testPosition, Quaternion testRotation)
    {
        Bounds b = GetHeldWorldBounds();

        Vector3 halfExtents = b.extents;

        // NOTE: b.center is based on current pose; we need center at test pose.
        // We'll compute offset from current object position to bounds center.
        Vector3 centerOffset = b.center - current.Position;
        Vector3 testCenter = testPosition + centerOffset;

        Collider[] hits = Physics.OverlapBox(
            testCenter,
            halfExtents,
            testRotation,
            overlapMask,
            QueryTriggerInteraction.Ignore
        );

        // Ignore self-colliders
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null) continue;

            // if the hit is part of the current held object, ignore it
            if (hits[i].transform.IsChildOf(current.HeldTransform)) continue;

            return true;
        }

        return false;
    }
    //*/
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
        if (current == null) return;
        
        // current.OnRelease();
        CleanupHold();
        print("are we holding? " + IsHolding.ToString());
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

    private void SetIgnorePlayerCollisions(HoldableV2 holdable, bool ignore)
    {

    }
}

