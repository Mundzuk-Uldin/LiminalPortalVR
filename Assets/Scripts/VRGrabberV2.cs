using UnityEngine;

public class VRGrabberV2 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("References")]
    [SerializeField] private HoldMotorV2 holdMotorV2;

    [Header("Input")]
    [SerializeField] private bool holdToGrab = true; // true: hold LMB, false: click to toggle
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    private bool toggledHolding;
    /// <summary>
    /// you click the left click of the mouse and it will trigger the action to hold something, thats it. when you release it stops triggering it
    /// </summary>
    void Awake()
    {
           if (holdMotorV2 == null)
            holdMotorV2 = FindFirstObjectByType<HoldMotorV2>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (holdMotorV2 == null) return;
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
        bool pressed = OVRInput.Get(OVRInput.Button.One);

        if (pressed)
        {
            // If not already holding anything, try to start.
            if (!holdMotorV2.IsHolding)
               holdMotorV2.TryStartHold();
        }
        else
        {
            // If holding, release when button released.
            if (holdMotorV2.IsHolding)
                holdMotorV2.EndHold();
        }
    }

    private void HandleToggleMode()
    {
        bool clicked = OVRInput.GetDown(OVRInput.Button.One);

        if (!clicked) return;

        if (holdMotorV2.IsHolding)
        {
            holdMotorV2.EndHold();
            toggledHolding = false;
            return;
        }

        holdMotorV2.TryStartHold();
        toggledHolding = holdMotorV2.IsHolding;
    }

    
}



