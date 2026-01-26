using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Portals;
using static OVRInput;

public class InputManager : MonoBehaviour {
    [SerializeField] float _mouseSensitivity = 3.0f;

    private float _snapTurnCooldown = 0f;
    [SerializeField] private float _snapTurnAngle = 30f;

    RigidbodyCharacterController _playerController;
    private bool _movementEnabled;

    void Awake() {
        _playerController = GetComponent<RigidbodyCharacterController>();
        _movementEnabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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


        if (Input.GetKeyDown(KeyCode.Space) || OVRInput.GetDown(OVRInput.Button.Three)) {
            _playerController.Jump();
        }

        //if (Input.GetKeyDown(KeyCode.Q)) {
        //    _playerController.ToggleNoClip();
        //}
    }

    void HandleMovement() {
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
    void HandleVRMovement()
    {
        if (!_movementEnabled) return;
/*         Debug.Log($"VR Input: {OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick)}");
        Debug.Log($"Controller Connected: {OVRInput.GetConnectedControllers()}"); */
        Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        
        // Only move if stick is pushed beyond dead zone
        if (primaryAxis.magnitude > 0.1f)
        {
            Vector3 moveDir = (Camera.main.transform.forward * primaryAxis.y) + 
                            (Camera.main.transform.right * primaryAxis.x);
            _playerController.Move(moveDir);
        }
    }

    void HandleVRRotation()
    {
        if (!_movementEnabled) return;
        
        Vector2 secondaryAxis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        
        if (_snapTurnCooldown <= 0f)
        {
            float xRotation = 0f;
            float yRotation = 0f;
            bool snapped = false;
            
            // Horizontal snap turn
            if (secondaryAxis.x > 0.7f) // Right
            {
                xRotation = _snapTurnAngle;
                snapped = true;
            }
            else if (secondaryAxis.x < -0.7f) // Left
            {
                xRotation = -_snapTurnAngle;
                snapped = true;
            }
            
            // Vertical snap turn
            if (secondaryAxis.y > 0.7f) // Up
            {
                yRotation = _snapTurnAngle * 0.5f;
                snapped = true;
            }
            else if (secondaryAxis.y < -0.7f) // Down
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

    void FixedUpdate() {
        HandleMovement();
        HandleVRMovement();
        HandleVRRotation();
    }
}
