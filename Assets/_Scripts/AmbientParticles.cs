using UnityEngine;

public class AmbientParticles : MonoBehaviour
{
    [SerializeField] private GameObject camera;
    [SerializeField] private bool isFollowing;

    private void Awake()
    {
        camera = Camera.main.gameObject;
    }

    private void Update()
    {
        if (!isFollowing) return;
        
        transform.position = camera.transform.position;
    }
}
