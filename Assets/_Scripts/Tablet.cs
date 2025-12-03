using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tablet : MonoBehaviour
{
    [SerializeField] private AudioClip obtainSFX;
    
    public void OnInteract()
    {
        ManagerPlayer.instance.PlayerTabletHelper.ObtainTablet();
        PlaySFX();
        Destroy(gameObject);
    }

    private void PlaySFX()
    {
        
    }
}
