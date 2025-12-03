using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Shapes;
using UnityEngine;
using Random = UnityEngine.Random;

public class CatCollectionTerminal : MonoBehaviour
{
    public static int CatCount { get; protected set; }
    
    [SerializeField] protected int catStreak;
    [SerializeField] protected int catQuota;
    [SerializeField] protected TextMeshProShapes catCountText;
    [SerializeField] protected TextMeshProShapes catQuotaText;
    
    [SerializeField] protected CatCollectionTerminalAnimationHelper animationHelper;
    [SerializeField] protected InteractableGeometry cctLeverInteractable;

    [field: SerializeField] public bool HasReachedQuota { get; protected set; }
    [field: SerializeField] public bool HasPower { get; set; } = true;
    [field: SerializeField] public bool IsOn { get; private set; }
    [SerializeField] protected bool isLeverInteractable;
    [SerializeField] protected bool isDoorOpen;
    [SerializeField] protected bool isInTransit;
    [SerializeField] protected bool isCopyCatError;

    [SerializeField] protected Transform enterTarget;
    [SerializeField] protected GameObject lever;
    [SerializeField] protected GameObject doorTop;

    [SerializeField] private GameObject collectedCatPrefab;
    
    [SerializeField] protected AudioClip leverOnSFX;
    [SerializeField] protected AudioClip leverOffSFX;

    public static Action OnCatSuccessfulCollection;

    protected ManagerCat ManagerCat;

    public static Action OnQuotaReached;
    
    private void OnEnable()
    {
        ManagerGame.OnGameStart += GameStart;
    }

    private void OnDisable()
    {
        ManagerGame.OnGameStart -= GameStart;
    }

    protected virtual void GameStart()
    {
        CatCount = 0;
        catQuotaText.text = ManagerGame.Instance.Difficulty.collectionGoal.ToString("D2");
    }

    protected virtual void Start()
    {
        animationHelper.PlayExpression(animationHelper.expression);
        ManagerCat = ManagerCat.instance;
    }

    public void ChangePower(bool state)
    {
        HasPower = state;
        cctLeverInteractable.HoverText = state ? cctLeverInteractable.ObjFlavorText.hoverText : "[no power]";
    }
    
    protected Coroutine StandbyCoroutine;
    protected Coroutine LeverCoroutine;
    protected Coroutine DoorCoroutine;
    
    public void OnLeverInteract()
    {
        if (HasReachedQuota) return;
        if (!isLeverInteractable) return;
        if (ManagerCat.FoundCats.Count == 0) return;

        if (!IsOn) // trying to turn on
        {
            if (HasPower)
            {
                IsOn = true;
                StartCoroutine(animationHelper.JitterTerminal());
                GenerateStandbyTargets();
                OnTurnOn();
                if (StandbyCoroutine != null) StopCoroutine(StandbyCoroutine);
            }
        }
        else // on -> off
        {
            IsOn = false;
            ResetAnimations();
            LeverCoroutine = StartCoroutine(OnLeverOff("interact"));
            StandbyCoroutine = StartCoroutine(StandbyDoorCloseTick());
        }
    }

    private void OnTurnOn()
    {
        IsOn = true;
        
        animationHelper.ApplyQuota();
        
        ManagerPlayer.instance.PlayerInputHelper.SetProcessing(false, ManagerPlayer.instance.PlayerInputHelper.RollCall, "catCollectionTerminal");
        ManagerCat.IsRollCalling = false;

        ResetAnimations();
        LeverCoroutine = StartCoroutine(OnLeverOn());
        
        StartCoroutine(CatCollectionTick());
            
        foreach (CatStateMachine stateMachine in ManagerCat.CatsStateMachine)
        {
            stateMachine.RequestStateChange(stateMachine.CatStatesDictionary[CatStateMachine.CatStates.Collecting]);
        }
    }

    protected void OnTurnOff()
    {
        catStreak = 0;
        
        IsOn = false;
        ManagerPlayer.instance.PlayerInputHelper.SetProcessing(true, ManagerPlayer.instance.PlayerInputHelper.RollCall, "catCollectionTerminal");
            
        foreach (CatStateMachine stateMachine in ManagerCat.CatsStateMachine)
        {
            stateMachine.RequestStateChange(stateMachine.CatStatesDictionary[CatStateMachine.CatStates.Idle]);
        }
        ManagerCat.ResetCatChainAiTarget();
    }

    private IEnumerator StandbyDoorCloseTick()
    {
        while (isInTransit)
        {
            yield return null;
        }

        OnTurnOff();
    }


    [SerializeField] private float catChainFirstStopDistance;
    [SerializeField] private float catChainStopDistance;
    [SerializeField] protected GameObject currentCatObj;
    [SerializeField] protected CatStateMachine currentCatStateMachine;
    [SerializeField] protected Cat currentCat;

    private void ResetCatChain()
    {
        List<GameObject> foundCats = new List<GameObject>(ManagerCat.FoundCats);
        List<CatMovementHelper> movementHelpers = new List<CatMovementHelper>(ManagerCat.CatsMovementHelpers);

        for (int i = 0; i < movementHelpers.Count; i++)
        {
            CatMovementHelper helper = movementHelpers[i];
            GameObject cat = foundCats[i];

            if (i == 0)
            {
                currentCatObj = cat;
                currentCatStateMachine = ManagerCat.CatsStateMachine[i];
                currentCat = ManagerCat.Cats[i];
                helper.AIDestinationSetter.target = isDoorOpen ? enterTarget : standbyTargets[0].transform;
                helper.FollowerEntity.stopDistance = catChainFirstStopDistance;
                
                if (isDoorOpen)
                {
                    isInTransit = true;
                    ManagerCat.RemoveCat(cat);
                }
            } else if (isDoorOpen && i < standbyTargets.Count)
            {
                helper.AIDestinationSetter.target = standbyTargets[i - 1].transform;
                helper.FollowerEntity.stopDistance = catChainFirstStopDistance;
            }
            else if (i < standbyTargets.Count)
            {
                helper.AIDestinationSetter.target = standbyTargets[i].transform;
                helper.FollowerEntity.stopDistance = catChainStopDistance;
            }
            else
            {
                helper.AIDestinationSetter.target = foundCats[i - 1].transform;
                helper.FollowerEntity.stopDistance = catChainStopDistance;
            }
        }
    }

    [SerializeField] private List<GameObject> standbyTargets;
    [SerializeField] private float standbyTargetDistance;
    [SerializeField] private GameObject standbyEntrance;
    [SerializeField] private int maxStandbyTargets;
    [SerializeField] private float standbyTargetCheckRadius = 0.5f;
    private void GenerateStandbyTargets()
    {
        foreach (GameObject target in standbyTargets)
        {
            Destroy(target);
        }
        standbyTargets.Clear();

        Vector3 dir = Vector3.left;
        dir = Vector3.ProjectOnPlane(dir, Vector3.up).normalized;

        Vector3 origin = standbyEntrance.transform.position + Vector3.up * 0.5f;
        float distance = standbyTargetDistance;

        int rotateAttempts = 0;
        float rotateAngle = -90f;

        for (int i = 0; i < maxStandbyTargets && i < ManagerCat.FoundCats.Count && rotateAttempts < 10; i++)
        {
            Ray ray = new Ray(origin, dir);
            bool blocked = Physics.Raycast(ray, out RaycastHit hit, distance, LayerUtility.Environment);

            if (!blocked)
            {
                GameObject target = new GameObject($"StandbyTarget_{i}");
                target.transform.position = origin + (dir * distance);
                target.transform.parent = transform;
                origin = target.transform.position;
                standbyTargets.Add(target);

                Debug.DrawRay(origin, dir * distance, Color.green, 3f); // GREEN = clear
            }
            else
            {
                if (rotateAttempts % 3 == 2) rotateAngle *= -1;
                dir = Quaternion.Euler(0, rotateAngle, 0) * dir;

                rotateAttempts++;
                Debug.DrawLine(origin, hit.point, Color.red, 3f); // RED = blocked
            }
        }
    }
    

    protected bool DoorStateLatched;
    private IEnumerator CatCollectionTick()
    {
        DoorStateLatched  = false;
        
        while (isInTransit || (IsOn && ManagerCat.FoundCats.Count > 0 && !isCopyCatError) && !HasReachedQuota)
        {
            // Latch
            if (!isDoorOpen && !DoorStateLatched )
            {
                DoorStateLatched  = true;
                ResetCatChain();
            }
            else if (isDoorOpen && DoorStateLatched )
            {
                DoorStateLatched = false;
                ResetCatChain();
            }
            
            if (isDoorOpen)
            {
                if (currentCat == null)
                {
                    ResetCatChain();
                }
                else
                {
                    if (Vector3.Distance(currentCat.transform.position, enterTarget.position) < 0.3f)
                    {
                        OnProcessCollection(currentCatObj, currentCatStateMachine, currentCat);
                    }
                }
            }
            
            yield return new WaitForFixedUpdate();
        }

        if ((!isCopyCatError && ManagerCat.FoundCats.Count == 0) || HasReachedQuota)
        {
            yield return new WaitForSeconds(1f);
            ResetAnimations();
            LeverCoroutine = StartCoroutine(OnLeverOff("autoclose"));
            
            StartCoroutine(StandbyDoorCloseTick());
            
            animationHelper.PlayExpression(CatCollectionTerminalAnimationHelper.Expressions.Heart);
        }
    }

    protected virtual void OnProcessCollection(GameObject catObj, CatStateMachine csm, Cat cat)
    {
        if (catObj.GetComponentInChildren<CatPhysicalModifierHelper>().isMimic)
        {
            StartCoroutine(OnMimicCollection(catObj, csm));
        }
        else
        {
            isInTransit = false;
            OnSuccessfulCollection();
            Destroy(catObj);
            
            StartCoroutine(SpitOutCatModel(cat));
        }
    }

    [SerializeField] private Transform depositPipe;
    [SerializeField] private float depositForce;
    [SerializeField] private float depositForceRandom;
    [SerializeField] private AudioClip pipeExitSFX;
    private IEnumerator SpitOutCatModel(Cat cat)
    {
        GameObject collectedCat = Instantiate(collectedCatPrefab, depositPipe.position, Quaternion.identity);
        Instantiate(cat.CatModel, collectedCat.transform.position, Quaternion.identity, collectedCat.transform);
        collectedCat.SetActive(false);
        
        yield return new WaitForSeconds(1f);
        
        ManagerSFX.Instance.PlaySFX(pipeExitSFX, depositPipe.position, 0.1f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.2f);
        
        collectedCat.SetActive(true);
        Vector3 forceDir = depositPipe.forward;
        
        float force = depositForce + Random.Range(-depositForceRandom, depositForceRandom);
        collectedCat.GetComponent<Rigidbody>().AddForce(forceDir * force, ForceMode.Impulse);
        
        if (ManagerElevator.Instance != null) ManagerElevator.Instance.ElevatorHelper.AddCatToElevator(collectedCat);
    }

    [SerializeField] private float rejectionForce;
    [SerializeField] protected AudioClip depositFailureSFX;
    protected virtual IEnumerator OnMimicCollection(GameObject cat, CatStateMachine csm)
    {
        animationHelper.PlayExpression(CatCollectionTerminalAnimationHelper.Expressions.Error);
        
        isInTransit = false;
        isCopyCatError = true;

        ManagerSFX.Instance.PlaySFX(depositFailureSFX, transform.position, 0.1f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX);
        
        yield return new WaitForSeconds(1f);
        
        OnTurnOff();
        
        csm.RequestStateChange(csm.CatStatesDictionary[CatStateMachine.CatStates.CopiedCollecting]);
        Vector3 forceDir = transform.position - cat.transform.position;
        forceDir = Vector3.ProjectOnPlane(forceDir, Vector3.up).normalized;
        cat.GetComponent<Rigidbody>().AddForce(forceDir * rejectionForce, ForceMode.Impulse);
        
        ManagerSFX.Instance.PlaySFX(pipeExitSFX, transform.position, 0.05f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.2f);
        
        yield return new WaitForSeconds(1f);
        ManagerCopyCat.Instance.ForceTransformCopyCat();
        
        ResetAnimations();
        LeverCoroutine = StartCoroutine(OnLeverOff("mimicCollection"));
        animationHelper.ApplyQuota();
        isCopyCatError = false;
    }

    [SerializeField] protected AudioClip depositSuccessSFX;
    protected virtual void OnSuccessfulCollection()
    {
        catStreak++;
        
        int catAdd = 1 + catStreak / 3;

        float pitch = 1 + (catStreak * 0.05f);
        ManagerSFX.Instance.PlaySFX(depositSuccessSFX, transform.position, 0.05f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: pitch, isRandomPitch: false);
        CatCount += catAdd;
        
        catCountText.text = CatCount.ToString("D2");
        
        OnCatSuccessfulCollection?.Invoke();
        
        if (CatCount >= ManagerGame.Instance.Difficulty.collectionGoal && !HasReachedQuota)
        {
            HasReachedQuota = true;
            OnQuotaReached?.Invoke();
            return;
        }
    }

    protected void ResetAnimations()
    {
        if (LeverCoroutine != null) StopCoroutine(LeverCoroutine);
        if (DoorCoroutine != null) StopCoroutine(DoorCoroutine);
    }

    private IEnumerator OnLeverOn()
    {
        ManagerSFX.Instance.PlaySFX(leverOnSFX, transform.position, 0.2f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX);
        
        isLeverInteractable = false;
        
        lever.transform.DOComplete();
        
        lever.transform.DOLocalRotate(new Vector3(-80, -90f, 90f), 0.2f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.2f);
        lever.transform.DOLocalRotate(new Vector3(-45f, 90f, -90f), 0.7f).SetEase(Ease.OutBounce);
        yield return new WaitForSeconds(0.7f);

        isLeverInteractable = true;
        DoorCoroutine = StartCoroutine(OnDoorOpen());
    }

    protected IEnumerator OnLeverOff(string source)
    {
        ManagerSFX.Instance.PlaySFX(leverOffSFX, transform.position, 0.2f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX);
        
        isLeverInteractable = false;
        
        lever.transform.DOComplete();

        float duration = 0.5f;
        lever.transform.DOLocalRotate(new Vector3(-90f, -90f, 90f), duration).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(duration);

        isLeverInteractable = true;
        DoorCoroutine = StartCoroutine(OnDoorClose());
    }

    [SerializeField] private AudioClip doorOpenSFX;
    [SerializeField] private AudioClip doorCloseSFX;
    private IEnumerator OnDoorOpen()
    {
        ManagerSFX.Instance.PlaySFX(doorOpenSFX, transform.position, 0.1f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX);
        
        doorTop.transform.DOComplete();
        
        doorTop.transform.DOMoveY(doorTop.transform.position.y + 0.1f, 0.3f).SetEase(Ease.OutBounce);
        yield return new WaitForSeconds(1f);
        doorTop.transform.DOLocalMoveY(0.7f, 0.6f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.6f);
        
        isDoorOpen = true;
        
    }

    private IEnumerator OnDoorClose()
    {
        ManagerSFX.Instance.PlaySFX(doorCloseSFX, transform.position, 0.1f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX);
        
        doorTop.transform.DOComplete();
        
        doorTop.transform.DOLocalMoveY(0.8f, 0.2f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.2f);
        doorTop.transform.DOLocalMoveY(0.0625f, 0.3f).SetEase(Ease.OutBounce);
        yield return new WaitForSeconds(0.3f);
        
        isLeverInteractable = true;
        isDoorOpen = false;
    }
}
