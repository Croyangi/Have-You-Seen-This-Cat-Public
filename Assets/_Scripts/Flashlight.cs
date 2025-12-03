using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [SerializeField] private AudioClip obtainSFX;
    
    public void OnInteract()
    {
        ManagerPlayer.instance.PlayerFlashlightHelper.ObtainFlashlight();
        PlaySFX();
        Destroy(gameObject);
    }

    private void PlaySFX()
    {
        
    }
}
