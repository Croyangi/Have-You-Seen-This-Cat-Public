using System;
using System.IO;
using Pathfinding;
using UnityEngine;
using Path = System.IO.Path;
using Random = UnityEngine.Random;

[SelectionBase]
public class Cat : InteractableObject, IInspectableObject
{
    [Header("References")]
    [field: SerializeField] public CatStateMachine StateMachine { get; private set; }
    [field: SerializeField] public CatPhysicalModifierHelper PhysicalModifierHelper { get; private set; }

    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Seeker seeker;
    [SerializeField] private FollowerEntity followerEntity;
    [field: SerializeField] public GameObject CatModel { get; private set; }
    
    [Header("Settings")]
    [field: SerializeField] public bool IsFound { get; private set; }

    public Action OnFound;

    public void SetIsFound(bool state)
    {
        IsFound = state;
        if (IsFound) OnFound?.Invoke();
    }

    protected override void Awake()
    {
        base.Awake();
        GraphMask traversableGraphMask = GraphMask.FromGraphName(PathfindingGraphUtility.CatPathfindingGraph);
        seeker.graphMask = traversableGraphMask;
        followerEntity.pathfindingSettings.graphMask = traversableGraphMask;
    }

    
    [SerializeField] private string catNamesFile = "CatNames.txt";
    protected override void Start()
    {
        base.Start();
        ManagerCat.instance.AddCat(gameObject);
        HoverText = TextFileReader.GetRandomLine(Application.streamingAssetsPath, catNamesFile);
    }
    
    public override void OnInteract()
    {
        if (!IsFound)
        {
            SetIsFound(true);
            ShowOutline();
            IsInspectable = true;
            ManagerCat.instance.FindCat(gameObject);
            
            // Find cat
            ManagerCat.instance.ResetCatChainAiTarget();
            StateMachine.RequestStateChange(StateMachine.CatStatesDictionary[CatStateMachine.CatStates.Idle]);

            if (ManagerCat.instance.IsRollCalling)
            {
                ManagerCat.instance.OnRollCall(false);
            }
        }
    }


    [SerializeField] private Color foundSelectedOutlineColor;
    public override void ShowOutline()
    {
        Color color = IsFound ? foundSelectedOutlineColor : selectedOutlineColor;
        foreach (Renderer r in renderers)
        {
            r.GetPropertyBlock(mpb, 1); // Get current properties for material 1
            mpb.SetColor("_OutlineColor", color); // Set for that material only
            r.SetPropertyBlock(mpb, 1);
        }
    }

    public override void HideOutline()
    {
        foreach (Renderer r in renderers)
        {
            r.GetPropertyBlock(mpb, 1); // Get current properties for material 1
            mpb.SetColor("_OutlineColor", originalOutlineColor); // Set for that material only
            r.SetPropertyBlock(mpb, 1);
        }
    }

    public void OnStartInspect()
    {
        IsBeingInteracted = true;

        // Force state into inspecting state
        StateMachine.RequestStateChange(StateMachine.CatStatesDictionary[CatStateMachine.CatStates.Inspecting]);
    }

    public void OnEndInspect()
    {
        IsBeingInteracted = false;
    }

    private void OnDestroy()
    {
        if (!Application.isPlaying) return;

        if (ManagerCat.instance != null)
        {
            ManagerCat.instance.RemoveCat(gameObject);
        }
    }
}
