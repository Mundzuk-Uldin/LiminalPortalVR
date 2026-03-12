using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PCCharacterController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform cameraTransform;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 0.12f; // tuned for pixels
    [SerializeField] private float pitchMin = -85f;
    [SerializeField] private float pitchMax = 85f;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController cc;
    private float pitch;
    private Vector3 verticalVel;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (cameraTransform == null)
            cameraTransform = GetComponentInChildren<Camera>()?.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
 
    void Update()
    {
        Look();
        Move();
    }

    void Look()
    {
        // Mouse delta (pixels per frame)
        Vector2 delta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;

        float yaw = delta.x * mouseSensitivity;
        float lookPitch = delta.y * mouseSensitivity;

        // yaw rotates the body
        transform.Rotate(Vector3.up * yaw);

        // pitch rotates the camera
        pitch -= lookPitch;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        if (cameraTransform != null)
            cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    void Move()
    {
        Vector2 moveInput = Vector2.zero;

        // WASD -> Vector2
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveInput.y += 1f;
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1f;
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1f;
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1f;
        }

        Vector3 move = (transform.right * moveInput.x + transform.forward * moveInput.y);
        if (move.sqrMagnitude > 1f) move.Normalize();

        cc.Move(move * moveSpeed * Time.deltaTime);

        // gravity
        if (cc.isGrounded && verticalVel.y < 0f)
            verticalVel.y = -2f;

        verticalVel.y += gravity * Time.deltaTime;
        cc.Move(verticalVel * Time.deltaTime);
    }
}
