using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Portals;
using static OVRInput;

public class InputManager : MonoBehaviour {
    [SerializeField] float _mouseSensitivity = 3.0f;



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

        float xRotation = Input.GetAxis("Mouse X") * _mouseSensitivity;
        float yRotation = Input.GetAxis("Mouse Y") * _mouseSensitivity;
        _playerController.Rotate(xRotation, yRotation);

        if (Input.GetKeyDown(KeyCode.Space)) {
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
        Debug.Log($"VR Input: {OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick)}");
        Debug.Log($"Controller Connected: {OVRInput.GetConnectedControllers()}");
        Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        
        // Only move if stick is pushed beyond dead zone
        if (primaryAxis.magnitude > 0.1f)
        {
            Vector3 moveDir = (Camera.main.transform.forward * primaryAxis.y) + 
                            (Camera.main.transform.right * primaryAxis.x);
            _playerController.Move(moveDir);
        }
    }

    void FixedUpdate() {
        HandleVRMovement();
    }
}
