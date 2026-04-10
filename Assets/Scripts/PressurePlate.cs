using UnityEngine;

/// <summary>
/// Attach to the big red button/pressure-plate collider (set as Trigger).
/// Any physics object entering the trigger counts as "pressing" it.
/// The assigned door opens while the button is held and closes when released.
/// </summary>
public class PressurePlate : MonoBehaviour
{
    [Tooltip("The door to open when this button is pressed.")]
    [SerializeField] private DoorOpen door;

    [Tooltip("If true the door stays open even after the object leaves.")]
    [SerializeField] private bool stayOpen = false;

    private int _contactCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        _contactCount++;
        if (_contactCount == 1 && door != null)
            door.OpenDoor();
    }

    private void OnTriggerExit(Collider other)
    {
        _contactCount = Mathf.Max(0, _contactCount - 1);
        if (_contactCount == 0 && !stayOpen && door != null)
            door.CloseDoor();
    }
}
