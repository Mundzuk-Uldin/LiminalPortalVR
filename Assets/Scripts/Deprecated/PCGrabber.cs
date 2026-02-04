using UnityEngine;
using UnityEngine.InputSystem;

public class PCGrabber : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private HoldMotor holdMotor;

    [Header("Raycast")]
    [SerializeField] private float maxGrabDistance = 5f;
    //[SerializeField] private LayerMask grabbableMask = ~0; // set in Inspector
    [SerializeField] private LayerMask grabbableMask;
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("Input")]
    [SerializeField] private bool holdToGrab = true; // true: hold LMB, false: click to toggle

    private bool toggledHolding;

    void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (holdMotor == null)
            holdMotor = FindFirstObjectByType<HoldMotor>();

        if (playerCamera == null)
            Debug.LogError("[PCGrabber] No Camera assigned and no Camera.main found.");

        if (holdMotor == null)
            Debug.LogError("[PCGrabber] No HoldMotor found in scene. Add one and reference it.");
    }

    void Update()
    {
        if (playerCamera == null || holdMotor == null)
            return;

        if (holdToGrab)
        {
            HandleHoldMode();
        }
        else
        {
            HandleToggleMode();
        }
    }

    private void HandleHoldMode()
    {
        bool pressed = Mouse.current != null && Mouse.current.leftButton.isPressed;

        if (pressed)
        {
            // If not already holding anything, try to start.
            if (!holdMotor.IsHolding)
                TryStartHold();
        }
        else
        {
            // If holding, release when button released.
            if (holdMotor.IsHolding)
                holdMotor.EndHold();
        }
    }

    private void HandleToggleMode()
    {
        bool clicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (!clicked) return;

        if (holdMotor.IsHolding)
        {
            holdMotor.EndHold();
            toggledHolding = false;
            return;
        }

        TryStartHold();
        toggledHolding = holdMotor.IsHolding;
    }
    private void TryStartHold()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        print("player pos:" + playerCamera.transform.position.ToString());
        if (!Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance, grabbableMask, triggerInteraction))
            return;

        // Extra safety: ensure this object actually supports holding logic
        Holdable holdable = hit.collider.GetComponentInParent<Holdable>();

        if (holdable == null)
            return;

        holdMotor.StartHold(holdable, hit.point);
    }

    /*
    private void TryStartHold()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance, grabbableMask, triggerInteraction))
        {
            // We expect Holdable on the parent "logic object"
            Holdable holdable = hit.collider.GetComponentInParent<Holdable>();

            if (holdable != null)
            {
                // Pass the hit point so the motor can optionally preserve grab offset later
                holdMotor.StartHold(holdable, hit.point);
            }
        }
    }
    */
    // Optional debug ray in Scene view
    void OnDrawGizmosSelected()
    {
        if (playerCamera == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * maxGrabDistance);
    }
}
