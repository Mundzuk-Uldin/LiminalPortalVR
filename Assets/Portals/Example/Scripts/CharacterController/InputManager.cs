using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Portals;
using static OVRInput;
using UnityEngine.XR;

public class InputManager : MonoBehaviour {
    [SerializeField] float _mouseSensitivity = 3.0f;

    private float _snapTurnCooldown = 0f;
    [SerializeField] private float _snapTurnAngle = 30f;

    RigidbodyCharacterController _playerController;
    [SerializeField] private bool _movementEnabled;
    [SerializeField] private bool _isVRMode = false;  // ← Add this

    void Awake() {
        _playerController = GetComponent<RigidbodyCharacterController>();
        _movementEnabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        StartCoroutine(HandleVRMode());
    }
    IEnumerator HandleVRMode()
    {
        yield return new WaitForSeconds(1.0f);
        _isVRMode = UnityEngine.XR.XRSettings.isDeviceActive;
    }

    void Update() {
        //if (Input.GetKeyDown(KeyCode.BackQuote)) {
        //    _movementEnabled = !_movementEnabled;
        //    if (_movementEnabled) {
        //        Cursor.lockState = CursorLockMode.Locked;
        //        Cursor.visible = false;
        //    } else {
        //        Cursor.lockState = CursorLockMode.None;
        //        Cursor.visible = true;
        //    }
        //}

        if (!_movementEnabled) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) || OVRInput.GetDown(OVRInput.Button.One)) {
            _playerController.Jump();
        }

        // Debug.Log("XRSettings.isDeviceActive = " + XRSettings.isDeviceActive);
        // Debug.Log("XRSettings.loadedDeviceName = " + XRSettings.loadedDeviceName);
        //if (Input.GetKeyDown(KeyCode.Q)) {
        //    _playerController.ToggleNoClip();
        //}
    }

    void HandlePCMovement() {
        Vector3 moveDir = Vector3.zero;
        bool moved = false;
        if (_movementEnabled) {
            if (Input.GetKey(KeyCode.W)) {
                moveDir += Camera.main.transform.forward;
                moved = true;
            }
            if (Input.GetKey(KeyCode.A)) {
                moveDir -= Camera.main.transform.right;
                moved = true;
            }
            if (Input.GetKey(KeyCode.S)) {
                moveDir -= Camera.main.transform.forward;
                moved = true;
            }
            if (Input.GetKey(KeyCode.D)) {
                moveDir += Camera.main.transform.right;
                moved = true;
            }

        }

        if (moved) {
            _playerController.Move(moveDir);
        }
    }
    // void HandleVRMovement()
    // {
    //     if (!_movementEnabled) return;
    //     /*      
    //     Debug.Log($"VR Input: {OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick)}");
    //     Debug.Log($"Controller Connected: {OVRInput.GetConnectedControllers()}"); 
    //     //*/
    //     Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
    //     InputDevice left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    //     InputDevice right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        
    //     // Only move if stick is pushed beyond dead zone
    //     if (primaryAxis.magnitude > 0.1f)
    //     {
    //         Vector3 moveDir = (Camera.main.transform.forward * primaryAxis.y) + 
    //                         (Camera.main.transform.right * primaryAxis.x);
    //         _playerController.Move(moveDir);
    //     }
    // }

    void HandleVRMovement()
    {
        if (!_movementEnabled) return;

        Vector2 primaryAxis = GetPrimaryMoveAxis();

        if (primaryAxis.magnitude > 0.1f)
        {
            Transform cam = Camera.main.transform;

            // Flatten camera vectors so movement stays horizontal
            Vector3 forward = cam.forward;
            Vector3 right = cam.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 moveDir = (forward * primaryAxis.y) + (right * primaryAxis.x);
            _playerController.Move(moveDir);
        }
    }

    void HandleVRHeadRotation()
    {
        if (!_movementEnabled) return;

        Quaternion headRot = Quaternion.identity;
        bool gotHeadRot = false;
        // ---- XR / OpenXR ----
        InputDevice head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        if (head.isValid && head.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion xrRot))
        {
            headRot = xrRot;
            gotHeadRot = true;
        }
        else
        {
            // ---- OVR fallback ----
            // headRot = OVRInput.GetLocalControllerRotation(OVRInput.Controller.Headset);
            // headRot = OVRInput.GetLocalControllerRotation(OVRInput.Controller. );
            // headRot = OVRManager.instance.centerEyeAnchor.localRotation;
                // OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();
                // if (rig != null)
                //     headRot = rig.centerEyeAnchor.localRotation;

                 OVRCameraRig rig = FindFirstObjectByType<OVRCameraRig>();
                if (rig != null && rig.centerEyeAnchor != null)
                {
                    headRot = rig.centerEyeAnchor.localRotation;
                    gotHeadRot = true;
                }
        }
         if (!gotHeadRot) return;
        Vector3 euler = headRot.eulerAngles;

        float xRotation = euler.y; // yaw
        float yRotation = euler.x; // pitch

        _playerController.Rotate(xRotation, yRotation);
    }

    void HandleVRRotation()
    {
        if (!_movementEnabled) return;

        Vector2 secondaryAxis = GetSecondaryTurnAxis();

        if (_snapTurnCooldown <= 0f)
        {
            float xRotation = 0f;
            float yRotation = 0f;
            bool snapped = false;

            // Horizontal snap turn
            if (secondaryAxis.x > 0.7f)
            {
                xRotation = _snapTurnAngle;
                snapped = true;
            }
            else if (secondaryAxis.x < -0.7f)
            {
                xRotation = -_snapTurnAngle;
                snapped = true;
            }

            // Vertical snap turn
            if (secondaryAxis.y > 0.7f)
            {
                yRotation = _snapTurnAngle * 0.5f;
                snapped = true;
            }
            else if (secondaryAxis.y < -0.7f)
            {
                yRotation = -_snapTurnAngle * 0.5f;
                snapped = true;
            }

            if (snapped)
            {
                _playerController.Rotate(xRotation, yRotation);
                _snapTurnCooldown = 0.35f;
            }
        }

        _snapTurnCooldown -= Time.fixedDeltaTime;
    }


    // void HandleVRRotation()
    // {
    //     if (!_movementEnabled) return;
        
    //     Vector2 secondaryAxis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        
    //     if (_snapTurnCooldown <= 0f)
    //     {
    //         float xRotation = 0f;
    //         float yRotation = 0f;
    //         bool snapped = false;
            
    //         // Horizontal snap turn
    //         if (secondaryAxis.x > 0.7f) // Right
    //         {
    //             xRotation = _snapTurnAngle;
    //             snapped = true;
    //         }
    //         else if (secondaryAxis.x < -0.7f) // Left
    //         {
    //             xRotation = -_snapTurnAngle;
    //             snapped = true;
    //         }
            
    //         // Vertical snap turn
    //         if (secondaryAxis.y > 0.7f) // Up
    //         {
    //             yRotation = _snapTurnAngle * 0.5f;
    //             snapped = true;
    //         }
    //         else if (secondaryAxis.y < -0.7f) // Down
    //         {
    //             yRotation = -_snapTurnAngle * 0.5f;
    //             snapped = true;
    //         }
            
    //         if (snapped)
    //         {
    //             _playerController.Rotate(xRotation, yRotation);
    //             _snapTurnCooldown = 0.35f;
    //         }
    //     }
        
    //     _snapTurnCooldown -= Time.fixedDeltaTime;
    // }



 private Vector2 GetPrimaryMoveAxis()
    {
        // OpenXR / Unity XR path: left-hand stick
        InputDevice left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (left.isValid &&
            left.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 xrAxis) &&
            xrAxis.sqrMagnitude > 0.0001f)
        {
            return xrAxis;
        }

        // Fallback to OVRInput
        Vector2 ovrAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        if (ovrAxis.sqrMagnitude > 0.0001f)
        {
            return ovrAxis;
        }

        return Vector2.zero;
    }

    private Vector2 GetSecondaryTurnAxis()
    {
        // OpenXR / Unity XR path: right-hand stick
        InputDevice right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (right.isValid &&
            right.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 xrAxis) &&
            xrAxis.sqrMagnitude > 0.0001f)
        {
            return xrAxis;
        }

        // Fallback to OVRInput
        Vector2 ovrAxis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        if (ovrAxis.sqrMagnitude > 0.0001f)
        {
            return ovrAxis;
        }

        return Vector2.zero;
    }

    void HandlePCRotation() {
        float xRotation = Input.GetAxis("Mouse X") * _mouseSensitivity;
        float yRotation = Input.GetAxis("Mouse Y") * _mouseSensitivity;

        _playerController.Rotate(xRotation, yRotation);
    }

    void FixedUpdate() {
        if (_isVRMode) {
            HandleVRMovement();
            HandleVRRotation();
            // HandleVRHeadRotation();
        } else {
            HandlePCMovement();
            HandlePCRotation();
        }
    }
}
