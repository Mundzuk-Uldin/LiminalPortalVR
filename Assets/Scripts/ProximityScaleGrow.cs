using UnityEngine;

public class ProximityScaleGrow : MonoBehaviour
{
    [SerializeField] Transform _target;
    [SerializeField] string _playerTag = "Player";

    [Header("Distance")]
    [SerializeField] float _startDistance = 5f;
    [SerializeField] float _fullDistance  = 1f;

    [Header("Scale")]
    [SerializeField] Vector3 _minScale  = Vector3.one;
    [SerializeField] Vector3 _maxScale  = new Vector3(10f, 10f, 10f);

    [Header("Curve")]
    [Tooltip("Exponent > 1 = slow start, fast finish. < 1 = fast start, slow finish.")]
    [SerializeField, Min(0.01f)] float _exponent = 3f;

    Transform _player;

    void Start()
    {
        if (_target == null)
            _target = transform;

        var playerObj = GameObject.FindGameObjectWithTag(_playerTag);
        if (playerObj != null)
            _player = playerObj.transform;

        _target.localScale = _minScale;
    }

    void Update()
    {
        if (_player == null) return;

        float dist = Vector3.Distance(_player.position, _target.position);

        // t = 0 at startDistance (or beyond), t = 1 at fullDistance (or closer)
        float t = Mathf.InverseLerp(_startDistance, _fullDistance, dist);

        // Exponential remap: raises t to a power so growth accelerates as you close in
        float tExp = Mathf.Pow(t, _exponent);

        _target.localScale = Vector3.LerpUnclamped(_minScale, _maxScale, tExp);
    }
}
