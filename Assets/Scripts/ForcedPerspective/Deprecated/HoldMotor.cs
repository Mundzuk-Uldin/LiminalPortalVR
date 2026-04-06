using UnityEngine;

public class HoldMotor : MonoBehaviour
{
    [Header("Aim Origin (PC camera / VR controller ray origin)")]
    [SerializeField] private Transform aimOrigin;

    [Header("Raycast")]
    [SerializeField] private LayerMask placementMask = ~0; // set in Inspector (World, etc.)
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;
    [SerializeField] private float noHitFallbackDistance = 10f;

    [Header("Smoothing")]
    [SerializeField] private bool smoothMotion = true;
    [SerializeField] private float positionLerpSpeed = 18f;
    [SerializeField] private float scaleLerpSpeed = 18f;

    public bool IsHolding => current != null;

    private int[] originalLayers;
    private Transform[] heldTransforms;
    [SerializeField] private int heldLayer = 2; // 2 = IgnoreRaycast by default in Unity

    private Holdable current;

    // Grab baseline
    private float initialDistance;
    private Vector3 initialScale;

    // Optional: keep the exact grabbed point (helps reduce "snap")
    private Vector3 grabbedWorldPoint;
    private Vector3 grabbedLocalPoint; // relative to object

    private Vector3 initialPivotOffsetFromHitWorld;

    private Quaternion initialRotationOffset; // in HoldMotor

    void Awake()
    {
        if (aimOrigin == null && Camera.main != null)
            aimOrigin = Camera.main.transform;

        if (aimOrigin == null)
            Debug.LogError("[HoldMotor] No aimOrigin set and no Camera.main found.");
        
    }

    // void Update()
    // {
    //     if (current == null || aimOrigin == null) return;

    //     TickHold();
    // }
    void LateUpdate()
    {
        if (current == null || aimOrigin == null) return;
        TickHold();
    }


    /// <summary>
    /// Start holding an object. hitPoint is where the ray hit the collider when grabbed.
    /// </summary>
    public void StartHold(Holdable holdable, Vector3 hitPoint)
    {
        if (holdable == null) return;

        // If already holding something, release it first (simple behavior)
        if (current != null)
            EndHold();

        current = holdable;

        current.SetPlayerRoot(this.transform); 

        // Baseline distance: from aim origin to the object's grabbed point (or pivot)
        grabbedWorldPoint = hitPoint;
        initialPivotOffsetFromHitWorld = current.transform.position - hitPoint;
        initialRotationOffset = Quaternion.Inverse(aimOrigin.rotation) * current.transform.rotation;
        Quaternion desiredRot = aimOrigin.rotation * initialRotationOffset;

        grabbedLocalPoint = current.transform.InverseTransformPoint(hitPoint);

        // initialDistance = Vector3.Distance(aimOrigin.position, grabbedWorldPoint);

        initialDistance = Vector3.Distance(aimOrigin.position, current.transform.position);

        initialDistance = Mathf.Max(0.01f, initialDistance); // avoid divide-by-zero

        initialScale = current.transform.localScale;


        current.OnGrab();
    }

    public void EndHold()
    {
        if (current == null) return;

        current.OnRelease();
        current = null;
    }

    private void TickHold()
    {

        // Raycast forward from aim origin
        Ray ray = new Ray(aimOrigin.position, aimOrigin.forward);

        float targetDistance = noHitFallbackDistance;

        float radius; //= current.GetCollisionRadius();
        // Vector3 targetPoint;

        // Find the first valid hit that is NOT the currently held object.
        RaycastHit[] hits = Physics.RaycastAll(ray, current.maxDistance, placementMask, triggerInteraction);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        bool found = false;
        RaycastHit bestHit = default;

        for (int i = 0; i < hits.Length; i++)
        {
            // Ignore the held object (anything under current.transform)
            if (hits[i].transform != null && hits[i].transform.IsChildOf(current.transform))
                continue;

            bestHit = hits[i];
            found = true;
            break;
        }

        if (found) // your "bestHit" after skipping the held object
        {
            radius = current.GetCollisionRadius();

            Vector3 safePoint = bestHit.point + bestHit.normal * (radius + current.surfacePadding);

            float safeDist = Vector3.Dot(safePoint - aimOrigin.position, aimOrigin.forward);
            targetDistance = Mathf.Max(0.01f, safeDist);
        }

        /*
        if (found)
        {
            Debug.Log("Using hit: " + bestHit.collider.name + " dist=" + bestHit.distance);

            float paddedDist = Mathf.Max(0.01f, bestHit.distance - current.surfacePadding);
            targetDistance = paddedDist;
        }
        else
        {
            // No hit other than the held object (or no hit at all)
            targetDistance = noHitFallbackDistance;
        }
        */

        /*
        // Raycast forward from aim origin
        Ray ray = new Ray(aimOrigin.position, aimOrigin.forward);
        

        
        float targetDistance = noHitFallbackDistance;

        float radius = current.GetCollisionRadius();
        // Vector3 targetPoint;


        if (Physics.Raycast(ray, out RaycastHit hit, current.maxDistance, placementMask, triggerInteraction))
        {
             Debug.Log("Hit: " + hit.collider.name + " dist=" + hit.distance);
            // Push off the surface a bit so we're not flush/clipping
            float paddedDist = Mathf.Max(0.01f, hit.distance - current.surfacePadding);
            targetDistance = paddedDist;
        }

        //  print(targetPoint);
        */

        // Clamp distance per-object
        targetDistance = Mathf.Clamp(targetDistance, current.minDistance, current.maxDistance);

        // Compute scale to preserve apparent size (Superliminal rule)
        float ratio = targetDistance / initialDistance;
        Vector3 desiredScale = initialScale * ratio;

        // Clamp scale per-object (clamp is to set a max and a min allowed)
        desiredScale.x = Mathf.Clamp(desiredScale.x, current.minScale, current.maxScale);
        desiredScale.y = Mathf.Clamp(desiredScale.y, current.minScale, current.maxScale);
        desiredScale.z = Mathf.Clamp(desiredScale.z, current.minScale, current.maxScale);

        // Compute position: keep the grabbed point aligned to the ray at target distance
        Vector3 desiredGrabPointOnRay = aimOrigin.position + aimOrigin.forward * targetDistance;

        
        print(desiredGrabPointOnRay.ToString());
        
        //v1:
        // We want the object's "grabbed local point" to land on desiredGrabPointOnRay.
        // After scaling, the local point moves, so we compute world offset accordingly.
        // Vector3 scaledLocal = Vector3.Scale(grabbedLocalPoint, desiredScale); // local point scaled
        // Vector3 desiredObjectPos = desiredGrabPointOnRay - (current.transform.rotation * scaledLocal);

        // ApplyTransform(desiredObjectPos, desiredScale);
        // current.SetHoldTarget(desiredObjectPos, desiredScale);


        // float ratio = targetDistance / initialDistance;
        // Vector3 desiredScale = initialScale * ratio;

        // clamp desiredScale as before...
        //v2:
        // Vector3 hitPointOnRay = aimOrigin.position + aimOrigin.forward * targetDistance;
        Vector3 hitPointOnRay = aimOrigin.position + aimOrigin.forward * targetDistance;
        print("target distance:" + targetDistance.ToString());
        // scale the pivot offset so it “grows” correctly as distance increases
        Vector3 desiredObjectPos = hitPointOnRay + (initialPivotOffsetFromHitWorld * ratio);

        current.SetHoldTarget(desiredObjectPos, desiredScale);
        
        //*/
    }

    private void ApplyTransform(Vector3 desiredPos, Vector3 desiredScale)
    {
        Transform t = current.transform;

        if (!smoothMotion)
        {
            t.position = desiredPos;
            t.localScale = desiredScale;
            return;
        }

        float posT = 1f - Mathf.Exp(-positionLerpSpeed * Time.deltaTime);
        float scaleT = 1f - Mathf.Exp(-scaleLerpSpeed * Time.deltaTime);

        t.position = Vector3.Lerp(t.position, desiredPos, posT);
        t.localScale = Vector3.Lerp(t.localScale, desiredScale, scaleT);
    }

    // Optional helper so PC/VR can change aim origin at runtime
    public void SetAimOrigin(Transform newAimOrigin)
    {
        aimOrigin = newAimOrigin;
    }

    private void SetLayerRecursive(Transform root, int layer)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;
    }

    private void CacheAndSetHeldLayer()
    {
        heldTransforms = current.GetComponentsInChildren<Transform>(true);
        originalLayers = new int[heldTransforms.Length];

        for (int i = 0; i < heldTransforms.Length; i++)
        {
            originalLayers[i] = heldTransforms[i].gameObject.layer;
            heldTransforms[i].gameObject.layer = heldLayer; // IgnoreRaycast
        }
    }

    private void RestoreHeldLayer()
    {
        if (heldTransforms == null || originalLayers == null) return;

        for (int i = 0; i < heldTransforms.Length; i++)
        {
            if (heldTransforms[i] != null)
                heldTransforms[i].gameObject.layer = originalLayers[i];
        }

        heldTransforms = null;
        originalLayers = null;
    }

}
