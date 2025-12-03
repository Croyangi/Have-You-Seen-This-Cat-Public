using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaitingRoomArea : MonoBehaviour, IDataPersistence
{
    [SerializeField] private AudioClip lightHumAmbience;
    [SerializeField] private WaitingRoomDeathCutscene waitingRoomDeathCutscene;
    [SerializeField] private TerminalMinigame startExpeditionMinigame;
    
    [SerializeField] private GameObject startExpeditionGoal;
    
    private void Start()
    {
        StartCoroutine(Initialize());
        ManagerElevator.Instance.Elevator.ElevatorTerminalInteractable.IsInteractable = true;
        ManagerElevator.Instance.Elevator.ElevatorTerminal.OnTerminalUnlocked += OnStartExpedition;
    }

    private void OnDisable()
    {
        ManagerElevator.Instance.Elevator.ElevatorTerminal.OnTerminalUnlocked -= OnStartExpedition;

        if (ManagerElevator.Instance.Elevator.ElevatorTerminal.TerminalMinigameBase != null)
        {
            ManagerElevator.Instance.Elevator.ElevatorTerminal.TerminalMinigameBase.OnTerminalMinigameGetResult -= ProcessMinigameResults;
        }
    }
    
    private IEnumerator Initialize()
    {
        ManagerSFX.Instance.PlayAmbienceSFX(lightHumAmbience, 0.05f);
        
        yield return new WaitForSeconds(2f);
        
        ManagerPlayer.instance.PlayerGoalHelper.AddGoal(Instantiate(startExpeditionGoal).GetComponent<StartExpeditionGoal>());
        
        ManagerElevator me = ManagerElevator.Instance;
        me.WaitingRoomDoor.Open();
        me.Elevator.ElevatorTerminal.TerminalBootStart();
        me.Elevator.ElevatorTerminal.TerminalMinigameBase.SetMinigames(new List<TerminalMinigame>{startExpeditionMinigame});
        me.Elevator.ElevatorTerminal.TerminalMinigameBase.OnTerminalMinigameGetResult += ProcessMinigameResults;
    }
    
    private void ProcessMinigameResults(ITerminalMinigameResult result)
    {
        if (result is ElevatorFloorMinigame.Result floorResult)
        {
            _isLoadingSave = floorResult.IsLoadingSave;
        }
    }
    
    public void LoadData(GameData data)
    {
        if (!data.hasDied)
        {
            ManagerPlayer.instance.PlayerVFXHelper.PlayerFadeToAwake();
        } else
        {
            data.expedition.isOngoing = false;
            waitingRoomDeathCutscene.PlayerDeathWaitingRoomCutscene();
        }
    }
    
    public void SaveData(ref GameData data)
    {
    }

    private bool _isLoadingSave;
    public void OnStartExpedition()
    {
        ManagerElevator.Instance.WaitingRoomDoor.Close();
        ManagerElevator.Instance.ElevatorHelper.WaitingRoomDescent(_isLoadingSave);
    }

    public void OnReplayTutorial()
    {
        SceneLoader.Load(SceneID.Tutorial);
    }
}
