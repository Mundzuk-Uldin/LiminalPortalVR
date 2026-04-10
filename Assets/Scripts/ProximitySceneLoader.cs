using UnityEngine;
using UnityEngine.SceneManagement;
using Portals;

[RequireComponent(typeof(SphereCollider))]
public class ProximitySceneLoader : MonoBehaviour
{
    [SerializeField] SceneField _sceneToLoad;
    [SerializeField] bool _loadAdditive = false;
    [SerializeField] string _playerTag = "Player";

    bool _hasLoaded;

    void Awake()
    {
        GetComponent<SphereCollider>().isTrigger = true;
    }

    void Update()
    {
        if (!_hasLoaded && Input.GetKeyDown(KeyCode.N))
        {
            LoadScene();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!_hasLoaded && other.CompareTag(_playerTag))
        {
            LoadScene();
        }
    }

    void LoadScene()
    {
        _hasLoaded = true;
        LoadSceneMode mode = _loadAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;
        SceneManager.LoadSceneAsync(_sceneToLoad, mode);
    }
}
