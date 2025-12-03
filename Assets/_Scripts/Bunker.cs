using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathfinding;
using UnityEngine;
using Path = System.IO.Path;
using Random = UnityEngine.Random;

public class Bunker : MonoBehaviour
{
    [SerializeField] private List<IndustrialLight> lights;
    [SerializeField] private List<IndustrialLight> exteriorLights;
    [SerializeField] private BunkerMetalDoor bunkerMetalDoor;
    [SerializeField] private CatCollectionTerminal catCollectionTerminal;
    [SerializeField] private BunkerMetalDoorTerminal internalBunkerTerminal;
    [SerializeField] private GenericTerminal exteriorBunkerTerminal;
    
    [SerializeField] private TerminalMinigame bunkerStandbyMinigame;
    [SerializeField] private TerminalMinigame sendElevatorMinigame;
    [SerializeField] private TerminalMinigame divertPowerMinigame;
    [SerializeField] private TerminalMinigame operateDoorMinigame;
    [SerializeField] private TerminalMinigame downloadCatDataMinigame;
    [SerializeField] private TerminalMinigame continueExpeditionMinigame;
    
    [SerializeField] private GraphUpdateScene bunkerTagGraph;
    [field: SerializeField] public Transform ConnectionPoint { get; private set; }

    [SerializeField] private AudioClip powerCutSFX;
    
    public static Action OnBunkerClose;

    [field: SerializeField] public bool HasPower { get; private set; }
    [field: SerializeField] public bool HasReachedQuota { get; private set; }

    [SerializeField] private string gameplayTipsDialogueFile = "GameplayTipsDialogue.txt";
    
    [SerializeField] private GameObject startExpeditionGoal;

    private void Awake()
    {
        HasPower = true;
    }

    private void OnEnable()
    {
        ManagerGame.OnGameStart += OnArrived;
        CatCollectionTerminal.OnQuotaReached += QuotaReached;
        
        internalBunkerTerminal.OnTerminalBootStart += SubscribeToMinigames;
        internalBunkerTerminal.OnTerminalUnlocked += OnDoorClose;
        internalBunkerTerminal.OnTerminalUnlocked += ResetTerminals;
        exteriorBunkerTerminal.OnTerminalUnlocked += ResetTerminals;

        ManagerGameplay.OnTimerEnd += OnTimerEnd;
    } 
    
    private void OnDisable()
    {
        if (internalBunkerTerminal.TerminalMinigameBase != null) internalBunkerTerminal.TerminalMinigameBase.OnTerminalMinigameEnd -= ProcessMinigameEnds;
        
        ManagerGame.OnGameStart -= OnArrived;
        CatCollectionTerminal.OnQuotaReached -= QuotaReached;
        
        internalBunkerTerminal.OnTerminalBootStart -= SubscribeToMinigames;
        internalBunkerTerminal.OnTerminalUnlocked -= OnDoorClose;
        internalBunkerTerminal.OnTerminalUnlocked -= ResetTerminals;
        exteriorBunkerTerminal.OnTerminalUnlocked -= ResetTerminals;
        
        ManagerGameplay.OnTimerEnd -= OnTimerEnd;
    }

    private void Start()
    {
        ToggleLights(false);
    }

    private void ResetTerminals()
    {
        bunkerMetalDoor.ResetTerminals();
    }
    
    private void OnTimerEnd() 
    {
        ResetTerminals();
        internalBunkerTerminal.IsProcessing = false;
        exteriorBunkerTerminal.IsProcessing = false;
        SetBunkerPower(false);
        bunkerMetalDoor.ToggleDoor();
    }

    private void OnDoorClose()
    {
        if (bunkerMetalDoor.MetalDoor.IsOpen) return;
        OnBunkerClose?.Invoke();
    }

    [ContextMenu("Test Quota Reached")]
    public void QuotaReached()
    {
        if (HasReachedQuota) return;
        HasReachedQuota = true;
        ManagerElevator.Instance.Elevator.ElevatorTerminal.OnTerminalUnlocked += OnElevatorTerminalUnlocked;
        internalBunkerTerminal.OnTerminalUnlocked += OnEndingTerminalUnlock;
        ManagerPlayer.instance.PlayerDialogueHelper.QueueDialogue("Excellent work. Send up those cats and get out of there.");
        internalBunkerTerminal.TerminalLock();
        StartCoroutine(QuotaReachedStandby());
        internalBunkerTerminal.TerminalBootStart();
    }

    private bool _endingTerminalUnlocked;
    private void OnEndingTerminalUnlock()
    {
        if (HasReachedQuota)
        {
            _endingTerminalUnlocked = true;
        }
    }

    private bool _isReturning;
    private void OnElevatorTerminalUnlocked()
    {
        ManagerGame.Instance.GameEnd();
        ManagerElevator.Instance.GameplayFloorDoor.Close();

        if (_isReturning)
        {
            ManagerElevator.Instance.ElevatorHelper.ReturnToSurface();
        }
        else
        {
            ManagerElevator.Instance.ElevatorHelper.ContinueExpedition();
        }
    }
    
    private void ProcessMinigameResults(ITerminalMinigameResult result)
    {
        if (result is ContinueExpeditionMinigame.Result floorResult)
        {
            _isReturning = floorResult.IsReturning;
        }
    }

    private IEnumerator QuotaReachedStandby()
    {
        while (!_endingTerminalUnlocked)
        {
            TerminalMinigameBase tmb = internalBunkerTerminal.TerminalMinigameBase;
            if (tmb != null && tmb.CurrentMinigame != null)
            {
                if (tmb.CurrentMinigame == divertPowerMinigame || tmb.CurrentMinigame == operateDoorMinigame ||
                    tmb.CurrentMinigame == downloadCatDataMinigame)
                {
                    tmb.OnMinigameSkip();
                    continue;
                }
            }

            yield return new WaitForFixedUpdate();
        }

        ManagerElevator me = ManagerElevator.Instance;
        while (me.ElevatorHelper.IsInAction) yield return new WaitForFixedUpdate();
        me.GameplayFloorDoor.Open();
        me.Elevator.ElevatorTerminalInteractable.IsInteractable = true;
        me.Elevator.ElevatorTerminal.TerminalBootStart();
        me.Elevator.ElevatorTerminal.TerminalMinigameBase.SetMinigames(new List<TerminalMinigame>{continueExpeditionMinigame});
        
        ManagerPlayer.instance.PlayerGoalHelper.AddGoal(Instantiate(startExpeditionGoal).GetComponent<StartExpeditionGoal>());
        ManagerPlayer.instance.PlayerGoalHelper.PingUI(10f);

        me.Elevator.ElevatorTerminal.TerminalMinigameBase.OnTerminalMinigameGetResult += ProcessMinigameResults;
    }

    private void SubscribeToMinigames()
    {
        internalBunkerTerminal.TerminalMinigameBase.OnTerminalMinigameEnd += ProcessMinigameEnds;
    }

    private void SetBunkerPower(bool state)
    {
        HasPower = state;
        catCollectionTerminal.ChangePower(HasPower);
        if (!HasPower)
        {
            ToggleLights(false);
        }
        else
        {
            FlickerLights(true);
            ManagerPlayer.instance.PlayerDialogueHelper.QueueDialogue(TextFileReader.GetRandomLine(Application.streamingAssetsPath, gameplayTipsDialogueFile));
        }
    }

    private void ProcessMinigameEnds(TerminalMinigame minigame)
    {
        if (minigame == sendElevatorMinigame)
        {
            ManagerElevator.Instance.ElevatorHelper.DeliverCats();
            return;
        }

        if (minigame == divertPowerMinigame)
        {
            SetBunkerPower(!HasPower);
            return;
        }

        if (minigame == operateDoorMinigame)
        {
            bunkerMetalDoor.ToggleDoor();
            return;
        }
        
        if (minigame == downloadCatDataMinigame)
        {
            ManagerCatModifier.instance.GeneratePhysicalModifiers();
            ManagerTablet.Instance.TabletAppMimicModifiers.RefreshPopulate();
            ManagerCat.instance.RefreshAllCatPhysicalModifiers();
            ManagerPlayer.instance.PlayerDialogueHelper.QueueDialogue("DNA mutation detected.\nDownloaded new data on tablet.");
            return;
        }
    }

    private void FixedUpdate()
    {
        if (!internalBunkerTerminal.IsOn) return;
        if (internalBunkerTerminal.TerminalMinigameBase == null) return;

        TerminalMinigameBase tmb = internalBunkerTerminal.TerminalMinigameBase;
        if (tmb.CurrentMinigame == null) return;
        if (tmb.CurrentMinigame == bunkerStandbyMinigame && IsBunkerPrepared())
        {
            tmb.OnMinigameSkip();
            return;
        }

        if (tmb.CurrentMinigame == sendElevatorMinigame && ManagerElevator.Instance.ElevatorHelper.CatObjs.Count <= 0)
        {
            tmb.OnMinigameSkip();
            return;
        }
        
        if (tmb.CurrentMinigame == downloadCatDataMinigame && ManagerCat.instance.FoundCats.Count > 0)
        {
            tmb.OnMinigameSkip();
            return;
        }
    }

    private bool IsBunkerPrepared()
    {
        if (catCollectionTerminal.IsOn) return false;
        if (ManagerElevator.Instance.ElevatorHelper.IsInAction) return false;
        return true;
    }
    
    [ContextMenu("On Arrived")]
    private void OnArrived()
    {
        FlickerLights(true);
    }

    private void FlickerLights(bool state)
    {
        foreach (IndustrialLight l in lights)
        {
            l.Flicker(state);
        }
        
        foreach (IndustrialLight l in exteriorLights)
        {
            l.Flicker(!state);
        }
    }

    private void ToggleLights(bool state)
    {
        foreach (IndustrialLight l in lights)
        {
            l.ToggleLights(state);
            if (!state && ManagerSFX.Instance != null) ManagerSFX.Instance.PlaySFX(powerCutSFX, l.transform.position, 0.05f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX);
        }
        
        foreach (IndustrialLight l in exteriorLights)
        {
            l.ToggleLights(!state);
            if (state && ManagerSFX.Instance != null) ManagerSFX.Instance.PlaySFX(powerCutSFX, l.transform.position, 0.05f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX);
        }
    }

    public void OnToggleDoor()
    {
        if (HasReachedQuota) return;
        bunkerMetalDoor.ToggleDoor();
        StartCoroutine(UpdateGraph());
    }

    private IEnumerator UpdateGraph()
    {
        yield return new WaitForSeconds(0.5f);
        bunkerTagGraph.Apply();
    }
}
