using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns pooled VFX objects above the player that streak down like comets.
/// Assign all 10 VFX trail prefabs in the Inspector.
/// </summary>
public class CometVFXPool : MonoBehaviour
{
    [Header("VFX Prefabs (assign all 10 trail prefabs)")]
    [SerializeField] private GameObject[] vfxPrefabs = new GameObject[10];

    [Header("Player Reference")]
    [Tooltip("Assign the player/camera transform. Falls back to Camera.main if left empty.")]
    [SerializeField] private Transform player;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2.5f;
    [SerializeField] private float spawnHeightMin = 10f;
    [SerializeField] private float spawnHeightMax = 20f;
    [SerializeField] private float spawnRadius = 15f;

    [Header("Comet Size")]
    [SerializeField] private float cometScale = 1f;

    [Header("Comet Movement")]
    [SerializeField] private float minSpeed = 10f;
    [SerializeField] private float maxSpeed = 35f;
    [SerializeField] private float despawnDistance = 45f;

    private readonly Queue<GameObject> _pool = new Queue<GameObject>();

    private class ActiveComet
    {
        public GameObject obj;
        public Vector3 velocity;
    }

    private readonly List<ActiveComet> _active = new List<ActiveComet>();

    void Awake()
    {
        if (player == null)
            player = Camera.main != null ? Camera.main.transform : transform;
    }

    void Start()
    {
        foreach (var prefab in vfxPrefabs)
        {
            if (prefab == null) continue;
            var obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            if (_pool.Count > 0)
                LaunchComet();
        }
    }

    void LaunchComet()
    {
        var obj = _pool.Dequeue();

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float height = Random.Range(spawnHeightMin, spawnHeightMax);
        Vector3 spawnPos = player.position + new Vector3(
            Mathf.Cos(angle) * spawnRadius,
            height,
            Mathf.Sin(angle) * spawnRadius);

        Vector3 inward = player.position - spawnPos;
        inward.y = 0f;
        inward = inward.normalized;
        float pitch = Random.Range(-20f, -5f);
        Vector3 dir = Quaternion.Euler(pitch, 0f, 0f) * inward;

        obj.transform.SetPositionAndRotation(spawnPos, Quaternion.LookRotation(dir));
        obj.transform.localScale = Vector3.one * cometScale;
        obj.SetActive(true);

        _active.Add(new ActiveComet { obj = obj, velocity = dir });
    }

    void Update()
    {
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var comet = _active[i];
            float dist = Vector3.Distance(comet.obj.transform.position, player.position);
            float t = Mathf.Clamp01(dist / spawnRadius);
            float currentSpeed = Mathf.Lerp(minSpeed, maxSpeed, t);
            comet.obj.transform.position += comet.velocity * (currentSpeed * Time.deltaTime);

            if (dist > despawnDistance)
            {
                comet.obj.SetActive(false);
                _pool.Enqueue(comet.obj);
                _active.RemoveAt(i);
            }
        }
    }
}
