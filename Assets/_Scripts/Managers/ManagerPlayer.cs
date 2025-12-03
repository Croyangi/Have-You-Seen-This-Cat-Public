using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class ManagerPlayer : MonoBehaviour, IDataPersistence
{
    [SerializeField] private GameObject playerPrefab;
    [field: SerializeField] public GameObject PlayerObj { get; private set; }
    [field: SerializeField] public GameObject PlayerHead { get; private set; }
    [field: SerializeField] public PlayerStateMachine PlayerStateMachine { get; private set; }
    [field: SerializeField] public PlayerInputHelper PlayerInputHelper { get; private set; }
    [field: SerializeField] public PlayerCameraHelper PlayerCameraHelper { get; private set; }
    [field: SerializeField] public PlayerDialogueHelper PlayerDialogueHelper { get; private set; }
    [field: SerializeField] public PlayerInventoryHelper PlayerInventoryHelper { get; private set; }
    [field: SerializeField] public PlayerMovementHelper PlayerMovementHelper { get; private set; }
    [field: SerializeField] public PlayerVFXHelper PlayerVFXHelper { get; private set; }
    [field: SerializeField] public PlayerFlashlightHelper PlayerFlashlightHelper { get; private set; }
    [field: SerializeField] public PlayerTabletHelper PlayerTabletHelper { get; private set; }
    [field: SerializeField] public PlayerCatHelper PlayerCatHelper { get; private set; }
    [field: SerializeField] public PlayerVisibilityHelper PlayerVisibilityHelper { get; private set; }
    [field: SerializeField] public PlayerUIHelper PlayerUIHelper { get; private set; }
    
    [field: SerializeField] public PlayerGoalHelper PlayerGoalHelper { get; private set; }
    [field: SerializeField] public PauseMenu PauseMenu { get; private set; }

    [SerializeField] private Tag tag_player;
    [SerializeField] private Tag tag_playerHead;
    [SerializeField] private bool dev_invincible;

    [SerializeField] private PlayerDeathHelper playerDeathHelper;

    // Manager
    public static ManagerPlayer instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Player Manager in the scene.");
        }

        instance = this;

        // Heavy method, but only called once for reference
        InitializePlayer();
    }

    private GameObject FindPlayer()
    {
        // Iterate over all GameObjects in the scene
        foreach (GameObject obj in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (obj.TryGetComponent<Tags>(out var tags) && tags.SearchTag(tag_player))
            {
                return obj;
            }
        }

        return null;
    }

    private GameObject FindPlayerHead(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.TryGetComponent<Tags>(out var tags) && tags.SearchTag(tag_playerHead))
            {
                return child.gameObject;
            }
        }

        return null;
    }

    private int _deaths;

    public void LoadData(GameData data)
    {
        _deaths = data.player.deaths;
    }

    private bool _hasDied;
    public void SaveData(ref GameData data)
    {
        data.player.deaths = _deaths;
        data.hasDied = _hasDied;
    }

    [ContextMenu("Kill Player")]
    public void PlayerDeath()
    {
        if (dev_invincible || _hasDied)
        {
            return;
        }

        _deaths++;
        _hasDied = true;
        playerDeathHelper.PlayerDeath();
        ManagerDataPersistence.Instance.SaveGame();
    }

    public void OnJumpscare()
    {
        PlayerStateMachine.RequestStateChange(PlayerStateMachine.PlayerStatesDictionary[PlayerStateMachine.PlayerStates.Jumpscare]);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            SceneManager.LoadScene("TitleScreen");
        }
    }

    private void InitializePlayer()
    {
        PlayerObj = FindPlayer();
        PlayerHead = FindPlayerHead(PlayerObj);
        PlayerStateMachine = PlayerObj.GetComponentInChildren<PlayerStateMachine>();
        PlayerInputHelper = PlayerObj.GetComponentInChildren<PlayerInputHelper>();
        PlayerCameraHelper = PlayerObj.GetComponentInChildren<PlayerCameraHelper>();
        PlayerInventoryHelper = PlayerObj.GetComponentInChildren<PlayerInventoryHelper>();
        PlayerMovementHelper = PlayerObj.GetComponentInChildren<PlayerMovementHelper>();
        PlayerVFXHelper = PlayerObj.GetComponentInChildren<PlayerVFXHelper>();
        PlayerFlashlightHelper = PlayerObj.GetComponentInChildren<PlayerFlashlightHelper>();
        PlayerTabletHelper = PlayerObj.GetComponentInChildren<PlayerTabletHelper>();
        PlayerCatHelper = PlayerObj.GetComponentInChildren<PlayerCatHelper>();
        PlayerVisibilityHelper = PlayerObj.GetComponentInChildren<PlayerVisibilityHelper>();
        PlayerDialogueHelper = PlayerObj.GetComponentInChildren<PlayerDialogueHelper>();
        PlayerUIHelper = PlayerObj.GetComponentInChildren<PlayerUIHelper>();
        PlayerGoalHelper = PlayerObj.GetComponentInChildren<PlayerGoalHelper>();
    }
}
