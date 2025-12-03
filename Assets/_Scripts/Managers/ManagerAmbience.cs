using UnityEngine;

public class ManagerAmbience : MonoBehaviour
{
    [SerializeField] private GameObject camera;
    [field: SerializeField] public bool IsParticlesFollowing { get; set; }
    [SerializeField] private GameObject ambientParticles;
    public static ManagerAmbience Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Ambience Manager in the scene.");
        }
        Instance = this;
        
        camera = Camera.main.gameObject;
        IsParticlesFollowing = true;
    }

    private void Update()
    {
        if (!IsParticlesFollowing) return;
        
        ambientParticles.transform.position = camera.transform.position;
    }
}
