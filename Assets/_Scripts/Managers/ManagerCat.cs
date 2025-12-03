using System;
using System.Collections;
using System.Collections.Generic;
using Febucci.UI.Core;
using UnityEngine;
using Pathfinding;
using Random = UnityEngine.Random;

public class ManagerCat : MonoBehaviour
{
    [Header("References")]
    [field: SerializeField] public List<GameObject> CatObjs { get; private set; }
    [field: SerializeField] public List<Cat> Cats { get; private set; }
    [field: SerializeField] public List<GameObject> FoundCats { get; private set; }
    [field: SerializeField] public List<GameObject> LostCats { get; private set; }
    
    [field: SerializeField] public List<CatStateMachine> CatsStateMachine { get; private set; }
    [field: SerializeField] public List<CatMovementHelper> CatsMovementHelpers { get; private set; }
    [field: SerializeField] public List<GameObject> RollCallTargets { get; private set; }
    [field: SerializeField] public bool IsRollCalling { get; set; }
    
    [SerializeField] private GameObject rollCallTargetPrefab;
    [SerializeField] private GameObject rollCallAvoidanceCol;
    [SerializeField] private GameObject rollCallAvoidanceColPrefab;
    [SerializeField] private GameObject rollCallLeaveArea;
    [SerializeField] private GameObject rollCallLeaveAreaPrefab;
    [SerializeField] private float catChainFirstStopDistance;
    [SerializeField] private float catChainStopDistance;
    
    // Manager
    public static ManagerCat instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Cat Manager in the scene.");
        }
        instance = this;
    }

    private void OnEnable()
    {
        ManagerElevator.OnArrived += SpreadSpawnCats;
        Bunker.OnBunkerClose += SpreadSpawnCats;
    }

    private void OnDisable()
    {
        ManagerElevator.OnArrived -= SpreadSpawnCats;
        Bunker.OnBunkerClose -= SpreadSpawnCats;
    }

    private void OnDestroy()
    {
        foreach (GameObject catObj in CatObjs)
        {
            Destroy(catObj);
        }
        
        foreach (GameObject target in RollCallTargets)
        {
            Destroy(target);
        }
        
        Cats.Clear();
        FoundCats.Clear();
        LostCats.Clear();
        CatsStateMachine.Clear();
        CatsMovementHelpers.Clear();
        RollCallTargets.Clear();
    }

    [SerializeField] private GameObject catPrefab;
    
    [field: SerializeField] public List<GameObject> NaturallySpawnedCats { get; private set; } 
    private void SpreadSpawnCats()
    {
        foreach (GameObject catObj in NaturallySpawnedCats)
        {
            Destroy(catObj);
        }
        NaturallySpawnedCats.Clear();
        
        GraphMask traversableGraphMask = GraphMask.FromGraphName(PathfindingGraphUtility.CopyCatPathfindingGraph);
        NNConstraint constraint = NNConstraint.Default;
        constraint.graphMask = traversableGraphMask;
        constraint.walkable = true;
        constraint.constrainTags = true;
        constraint.tags = ~(1 << 3); // exclude tag 3
        
        for (int i = 0; i < ManagerGame.Instance.Difficulty.naturalCatSpawnCount; i++)
        {
            Vector3 point = (Vector3) AstarPath.active.data.recastGraph.RandomPointOnSurface(constraint);
            GameObject cat = Instantiate(catPrefab, point, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
            NaturallySpawnedCats.Add(cat);
        }
    }

    public void RefreshAllCatPhysicalModifiers()
    {
        foreach (GameObject lostCat in LostCats)
        {
            lostCat.GetComponent<Cat>().PhysicalModifierHelper.RefreshPhysicalModifiers();
        }
    }

    public void AddCat(GameObject cat)
    {
        CatObjs.Add(cat);
        LostCats.Add(cat);
    }

    public void FindCat(GameObject cat)
    {
        NaturallySpawnedCats.Remove(cat);
        LostCats.Remove(cat);
        FoundCats.Add(cat);
        OnCatsFound(cat);
    }
    
    public void RemoveCat(GameObject cat)
    {
        CatObjs.Remove(cat);
        LostCats.Remove(cat);
        FoundCats.Remove(cat);
        OnCatsRemoved(cat);
    }

    private void OnCatsFound(GameObject catObj)
    {
        if (catObj.GetComponent<Cat>() != null)
        {
            Cats.Add(catObj.GetComponent<Cat>());
        }
        
        if (catObj.GetComponentInChildren<CatStateMachine>() != null)
        {
            CatsStateMachine.Add(catObj.GetComponentInChildren<CatStateMachine>());
        }
        
        if (catObj.GetComponentInChildren<CatMovementHelper>() != null)
        {
            CatsMovementHelpers.Add(catObj.GetComponentInChildren<CatMovementHelper>());
        }
    }
    
    private void OnCatsRemoved(GameObject catObj)
    {
        if (catObj.GetComponent<Cat>() != null)
        {
            Cats.Remove(catObj.GetComponent<Cat>());
        }
        
        if (catObj.GetComponentInChildren<CatStateMachine>() != null)
        {
            CatsStateMachine.Remove(catObj.GetComponentInChildren<CatStateMachine>());
        }
        
        if (catObj.GetComponentInChildren<CatMovementHelper>() != null)
        {
            CatsMovementHelpers.Remove(catObj.GetComponentInChildren<CatMovementHelper>());
        }
    }

    public void ResetCatChainAiTarget()
    {
        for (int i = 0; i < CatsMovementHelpers.Count; i++)
        {
            CatMovementHelper helper = CatsMovementHelpers[i];
            if (i == 0)
            {
                helper.AIDestinationSetter.target = ManagerPlayer.instance.PlayerHead.transform;
                helper.FollowerEntity.stopDistance = catChainFirstStopDistance;
            }
            else
            {
                helper.AIDestinationSetter.target = FoundCats[i-1].transform;
                helper.FollowerEntity.stopDistance = catChainStopDistance;
            }
        }
    }

    public void OnLeaveRollCallArea()
    {
        Destroy(rollCallLeaveArea);
        IsRollCalling = false;
        OnRollCall(false);
    }

    public void OnScatterCats()
    {
        List<CatStateMachine> csms = new List<CatStateMachine>();
        
        for (int i = 0; i < FoundCats.Count / 2; i++)
        {
            CatStateMachine stateMachine = CatsStateMachine[i];
            csms.Add(stateMachine);
        }

        foreach (CatStateMachine stateMachine in csms)
        {
            stateMachine.RequestStateChange(stateMachine.CatStatesDictionary[CatStateMachine.CatStates.Scattering]);
        }
        
        ResetCatChainAiTarget();
    }

    private void SetRollCallOnCats(bool value)
    {
        foreach (CatStateMachine stateMachine in CatsStateMachine)
        {
            stateMachine.RequestStateChange(stateMachine.CatStatesDictionary[value ? CatStateMachine.CatStates.RollCall : CatStateMachine.CatStates.Idle]);
        }
    }

    private bool GetCanRollCall()
    {
        if (FoundCats.Count > 0)
        {
            return true;
        }

        return false;
    }

    public void OnRollCall(bool state = false)
    {
        if (!GetCanRollCall()) return;

        IsRollCalling = state;
        SetRollCallOnCats(IsRollCalling);
        
        if (IsRollCalling)
        {
            GenerateRollCall(FoundCats.Count);
        }
        else
        {
            foreach (GameObject obj in RollCallTargets)
            {
                Destroy(obj);
            }
            RollCallTargets.Clear();
                
            Destroy(rollCallLeaveArea);
            ResetCatChainAiTarget();
        }
    }
    
    [SerializeField] private float rollCallSphereRadius = 0.2f;
    
    /// <summary>
    /// Returns a list of valid positions for roll call.
    /// </summary>
    private List<Vector3> GetValidRollCallPositions(int desiredCount, float radius)
    {
        Vector3 origin = ManagerPlayer.instance.PlayerObj.transform.position + Vector3.up * rollCallSphereRadius;
        List<Vector3> validPositions = new List<Vector3>();

        float angleSpacing = 360f / desiredCount;
        float angleOffset = UnityEngine.Random.Range(0f, 360f); // Randomize start angle to help distribute better

        for (int i = 0; i < desiredCount; i++)
        {
            float angle = angleOffset + i * angleSpacing;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Ray ray = new Ray(origin, direction);

            if (!Physics.SphereCast(ray, rollCallSphereRadius, out RaycastHit hit, radius, LayerUtility.Environment))
            {
                Vector3 pos = GetRollCallPosition(angle, radius, ManagerPlayer.instance.PlayerObj.transform.position);
                validPositions.Add(pos);

                Debug.DrawRay(origin, direction * radius, Color.cyan, 5f);
            }
            else
            {
                Debug.DrawLine(origin, hit.point, Color.red, 2f);
            }
        }

        return validPositions;
    }

    /// <summary>
    /// Generates the Roll Call Area to detect leaving it.
    /// </summary>
    private void GenerateRollCallArea(GameObject player, float radius)
    {
        Vector3 origin = player.transform.position;
        rollCallLeaveArea = Instantiate(rollCallLeaveAreaPrefab, origin, Quaternion.identity);
        rollCallLeaveArea.GetComponent<SphereCollider>().radius = radius * 1.5f;
    }
    
    
    /// <summary>
    /// Generates and sets AI Pathing targets for cats. 
    /// </summary>
    private void GenerateRollCall(int foundCats)
    {
        int catsRemaining = foundCats;
    
        float radius = 0.3f;
        float catSpace = 0.5f;
        float walkingSpace = 0.5f;
        float radiusIncrement = 1.5f;

        int safetyCounter = 0;
        int safetyLimit = CatObjs.Count;
    
        while (catsRemaining > 0 && safetyCounter < safetyLimit)
        { 
            radius += radiusIncrement;
            safetyCounter++;

            int totalCats = GetRollCallTotalCats(radius, catSpace, walkingSpace);
            totalCats = Mathf.Clamp(totalCats, 0, catsRemaining);
            List<Vector3> rollCallPositions = GetValidRollCallPositions(totalCats, radius);


            // Avoid infinite loop
            if (rollCallPositions.Count == 0)
            {
                Debug.LogWarning($"RollCall: No valid positions found at radius {radius}. Breaking early.");
                break;
            }

            catsRemaining -= rollCallPositions.Count;

            foreach (Vector3 rollCallPosition in rollCallPositions)
            {
                GameObject rollCallTarget = Instantiate(rollCallTargetPrefab, rollCallPosition, Quaternion.identity);
                RollCallTargets.Add(rollCallTarget);
            }
        }

        // Safely assign targets to cats that were actually placed
        int assignCount = Mathf.Min(foundCats, RollCallTargets.Count);
        for (int i = 0; i < assignCount; i++)
        {
            CatMovementHelper helper = CatsMovementHelpers[i];
            helper.AIDestinationSetter.target = RollCallTargets[i].transform;
        }

        GenerateRollCallArea(ManagerPlayer.instance.PlayerObj, radius);
    }
    
    /// <summary>
    /// Returns a Vector3 for a Roll Call position.
    /// </summary>
    private Vector3 GetRollCallPosition(float angle, float distance, Vector3 playerPos)
    {
        float radians = angle * Mathf.Deg2Rad;
        float dx = distance * Mathf.Sin(radians);
        float dz = distance * Mathf.Cos(radians);
        Vector3 newPosition = new Vector3(playerPos.x + dx, playerPos.y, playerPos.z + dz);
        return newPosition;
    }

    /// <summary>
    /// Returns the maximum possible amount of cats.
    /// </summary>
    private int GetRollCallTotalCats(float radius, float catSpace, float walkingSpace)
    {
        float circumference = 2 * Mathf.PI * radius;
        float catSize = catSpace + (2 * walkingSpace);
        int totalCats = (int)Mathf.Floor(circumference / catSize);

        if (totalCats * catSize > circumference)
        {
            totalCats--;
        }

        float totalSpaceTaken = totalCats * catSize;

        return totalCats;
    }
}
