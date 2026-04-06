using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    [Tooltip("How many degrees the door rotates when opened. Use negative to swing the other way.")]
    [SerializeField] private float openAngle = 90f;

    [Tooltip("Rotation speed in degrees per second.")]
    [SerializeField] private float rotationSpeed = 120f;

    [Tooltip("Start in the open position.")]
    [SerializeField] private bool startOpen = false;

    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private Quaternion _targetRotation;
    private bool _isOpen;

    void Start()
    {
        _closedRotation = transform.localRotation;
        _openRotation = _closedRotation * Quaternion.Euler(0f, openAngle, 0f);

        _isOpen = startOpen;
        _targetRotation = _isOpen ? _openRotation : _closedRotation;
        transform.localRotation = _targetRotation;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            ToggleDoor();

        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation, _targetRotation, rotationSpeed * Time.deltaTime);
    }

    /// <summary>Toggles the door between open and closed.</summary>
    public void ToggleDoor()
    {
        _isOpen = !_isOpen;
        _targetRotation = _isOpen ? _openRotation : _closedRotation;
    }

    /// <summary>Opens the door.</summary>
    public void OpenDoor()
    {
        _isOpen = true;
        _targetRotation = _openRotation;
    }

    /// <summary>Closes the door.</summary>
    public void CloseDoor()
    {
        _isOpen = false;
        _targetRotation = _closedRotation;
    }
}
