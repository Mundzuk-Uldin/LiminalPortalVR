using UnityEngine;

public class TeleportObjToSpawn : MonoBehaviour
{
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private Vector3 _startScale;

    void Start()
    {
        _startPosition = transform.position;
        _startRotation = transform.rotation;
        _startScale = transform.localScale;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            transform.SetPositionAndRotation(_startPosition, _startRotation);
            transform.localScale = _startScale;
        }
    }
}
