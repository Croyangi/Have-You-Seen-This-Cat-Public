using UnityEngine;

public class DeathBounds : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        ManagerPlayer.instance.PlayerDeath();
    }
}
