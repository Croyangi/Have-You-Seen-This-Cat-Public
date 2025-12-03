using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class PlayerDeathHelper : MonoBehaviour
{
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private Canvas PPCanvas;
    [SerializeField] private AudioClip cameraScreenCrackSFX;
    
    private void Awake()
    {
        deathScreen.SetActive(false);
        
        PPCanvas.renderMode = RenderMode.ScreenSpaceCamera;

        foreach (Camera cam in Camera.allCameras)
        {
            if (cam.tag == "UICamera" || cam.gameObject.name == "UI")
            {
                PPCanvas.worldCamera = cam;
                return;
            }
        }
    }
    

    [ContextMenu("Player Death")]
    public void PlayerDeath()
    {
        StartCoroutine(HandlePlayerDeath());
    }

    private IEnumerator HandlePlayerDeath()
    {
        ManagerSFX.Instance.PlayRawSFX(cameraScreenCrackSFX, 0.3f, false, ManagerAudioMixer.Instance.AMGSFX, 0.01f);
        deathScreen.SetActive(true);
        yield return new WaitForFixedUpdate();
        SceneLoader.Load(SceneID.WaitingRoom);
    }
}
