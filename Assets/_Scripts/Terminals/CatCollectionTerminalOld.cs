using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[SelectionBase]
public class CatCollectionTerminalOld : MonoBehaviour
{
    /*
    [SerializeField] private Difficulty difficulty;
    
    [SerializeField] private int catCount;
    [SerializeField] private int catStreak;
    [SerializeField] private TextMeshProUGUI catCountText;
    
    [SerializeField] private GameObject unlockedObjs;

    [SerializeField] private float autoLockTimer;
    [SerializeField] private float autoLockTimerSet;
    
    [SerializeField] private bool isAutoLocking;

    [SerializeField] private GameObject trashChute;
    
    [SerializeField] private AudioClip hatchUnlock;
    [SerializeField] private AudioClip hatchOpen;
    [SerializeField] private AudioClip hatchClose;
    [SerializeField] private AudioClip hatchDeposit;
    [SerializeField] private Transform hatchAudioSpot;
    [SerializeField] private AudioClip[] depositBonuses;
    
    [Header("Lights")]
    [SerializeField] private MeshRenderer redLightMesh;
    [SerializeField] private MeshRenderer greenLightMesh;
    [SerializeField] private GameObject redLight;
    [SerializeField] private GameObject greenLight;
    [SerializeField] private Material redLightMaterial;
    [SerializeField] private Material greenLightMaterial;
    [SerializeField] private Material offLightMaterial;

    private void Awake()
    {
        SetLightOff();
        unlockedObjs.SetActive(false);
        autoLockTimer = autoLockTimerSet;
        isAutoLocking = true;
        SetText();
    }

    private void OnTriggerEnter(Collider other)
    {
        isAutoLocking = false;
        autoLockTimer = autoLockTimerSet;
    }

    private void OnTriggerExit(Collider other)
    {
        isAutoLocking = true;
    }

    private void FixedUpdate()
    {
        if (autoLockTimer <= 0)
        {
            OnTerminalLock();
            autoLockTimer = autoLockTimerSet;
        } else if (isUnlocked && isAutoLocking)
        {
            autoLockTimer = Mathf.Max(0, autoLockTimer -= Time.fixedDeltaTime);
        }
    }
    
    public void OnInteract()
    {
        if (!isTransitioning)
        {
            if (isOn)
            {
                OnTerminalLockedInteraction();
            }
            else
            {
                OnTerminalBootStart();
            }
        }
    }
    
    public void OnInteractTrashChute()
    {
        if (isUnlocked)
        {
            OnTerminalUnlockedInteraction();
        }
    }

    public void OnTerminalLockedInteraction()
    {
        terminalMinigameBase.ProcessFocus();
        StopAllCoroutines();
        
        if (terminalMinigameBase.isFocused)
        {
            StartCoroutine(AdjustPlayerToTerminal(0.5f));
            isTransitioning = true;
            ManagerPlayer.instance.PlayerInputHelper.SetProcessing(false, ManagerPlayer.instance.PlayerInputHelper.Terminal, "terminal");
            terminalViewCamera.SetActive(true);
        }
        else
        {
            isTransitioning = true;
            ManagerPlayer.instance.PlayerInputHelper.SetProcessing(true, ManagerPlayer.instance.PlayerInputHelper.Terminal, "terminal");
            terminalViewCamera.SetActive(false);
        }
        
        StartCoroutine(ProcessTransition(0.5f));
    }

    public void OnTerminalUnlockedInteraction()
    {
        PlayerInventoryHelper inventory = ManagerPlayer.instance.PlayerInventoryHelper;

        if (inventory.heldItem == null) return;
        if (!inventory.heldItem.TryGetComponent<InventoryItemHolder>(out var inventoryItemHolder)) return;

        if (inventory.heldItem.GetComponentInChildren<CatPhysicalModifierHelper>() != null)
        {
            if (inventory.heldItem.GetComponentInChildren<CatPhysicalModifierHelper>().isMimic)
            {
                PlayerInventoryHelper playerInventory = ManagerPlayer.instance.PlayerInventoryHelper;
                playerInventory.DropHeldItem();
                ManagerCopyCat.Instance.ForceTransformCopyCat();
                return;
            } 
        }
        
        if (inventoryItemHolder.inventoryItem == inventoryItem_cat)
        {
            OnCatDeposit();
        }
    }

    private void OnCatDeposit()
    {
        ManagerSFX.Instance.PlaySFX(hatchDeposit, hatchAudioSpot.position, 0.1f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        
        PlayerInventoryHelper inventory = ManagerPlayer.instance.PlayerInventoryHelper;
        inventory.RemoveHeldItem();
        
        autoLockTimer = autoLockTimerSet;

        catStreak++;

        AudioClip bonusSFX;
        int catAdd = 1;
        if (catStreak >= 6)
        {
            bonusSFX = depositBonuses[2];
            catAdd += 2;
        } else if (catStreak >= 3)
        {
            bonusSFX = depositBonuses[1];
            catAdd++;
        }
        else
        {
            bonusSFX = depositBonuses[0];
        }
        
        ManagerSFX.Instance.PlaySFX(bonusSFX, transform.position, 0.05f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX);
        catCount += catAdd;
        
        SetText();
        
        if (catCount >= difficulty.collectionGoal)
        {
            SceneManager.LoadScene("GameOver");
        }
    }

    private void SetText()
    {
        catCountText.text = catCount + "/" + difficulty.collectionGoal;
    }
    
    [ContextMenu("Set Off")]
    private void SetLightOff()
    {
        redLightMesh.material = redLightMaterial;
        greenLightMesh.material = offLightMaterial;
        
        redLight.SetActive(true);
        greenLight.SetActive(false);
    }
    
    [ContextMenu("Set On")]
    private void SetLightOn()
    {
        redLightMesh.material = offLightMaterial;
        greenLightMesh.material = greenLightMaterial;
        
        redLight.SetActive(false);
        greenLight.SetActive(true);
    }
    
    private IEnumerator OpenChute()
    {
        SFXObject sfx0 = ManagerSFX.Instance.PlaySFX(hatchUnlock, hatchAudioSpot.position, 0.1f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        ManagerSFX.Instance.ApplyLowPassFilter(sfx0);
        
        SetLightOn();
        trashChute.transform.DOComplete();
        trashChute.transform.DOLocalRotate(new Vector3(-125f, 0f, 0f), 1f).SetEase(Ease.OutBounce).SetDelay(0.5f);
        
        yield return new WaitForSeconds(0.5f);
        
        SFXObject sfx1 = ManagerSFX.Instance.PlaySFX(hatchOpen, hatchAudioSpot.position, 0.1f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        ManagerSFX.Instance.ApplyLowPassFilter(sfx1);
    }
    
    private IEnumerator CloseChute()
    {
        SetLightOff();
        trashChute.transform.DOComplete();
        trashChute.transform.DOLocalRotate(new Vector3(-130f, 0f, 0f), 0.2f).SetEase(Ease.OutCubic);
        trashChute.transform.DOLocalRotate(new Vector3(-90f, 0f, 0f), 0.5f).SetEase(Ease.OutBounce).SetDelay(0.2f);
        
        SFXObject sfx = ManagerSFX.Instance.PlaySFX(hatchClose, hatchAudioSpot.position, 0.1f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        ManagerSFX.Instance.ApplyLowPassFilter(sfx);

        yield return null;
    }

    public void OnTerminalUnlock()
    {
        terminalMinigameBase.isFocused = false;
        ManagerPlayer.instance.PlayerInputHelper.SetProcessing(true, ManagerPlayer.instance.PlayerInputHelper.Terminal, "terminal");
        terminalViewCamera.SetActive(false);
        TerminalMinigame.OnMinigameUnfocus();
        isUnlocked = true;
        SetLightOn();
        
        StopAllCoroutines();
        StartCoroutine(ProcessTransition(0.5f));
        
        StartCoroutine(OpenChute());
        
        unlockedObjs.SetActive(true);
    }

    [ContextMenu("Lock")]
    public void OnTerminalLock()
    {
        isUnlocked = false;
        isOn = false;
        SetLightOff();
        
        StartCoroutine(CloseChute());
        catStreak = 0;
        
        unlockedObjs.SetActive(false);
    }
    */
}
