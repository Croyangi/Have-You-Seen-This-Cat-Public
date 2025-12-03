using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using TMPro;
using Random = UnityEngine.Random;

public class ManagerCatModifier : MonoBehaviour
{
    [Header("Name Settings")] 
    [SerializeField] private int namesCount;
    [SerializeField] private string namesFilePath;
    [field: SerializeField] public List<string> names { get; private set; }
    
    [Header("Physical Modifier Settings")] 
    [SerializeField] private int invalidPhysicalModifiersPoolCount;
    [SerializeField] private int validPhysicalModifiersPoolCount;
    [SerializeField] private string physicalModifiersFilePath;
    [SerializeField] private string defaultPhysicalModifiersFilePath;

    [SerializeField] private int invalidPhysicalModifiersCount;
    [SerializeField] private int validPhysicalModifiersCount;
    [field: SerializeField] public List<CatPhysicalModifier> DefaultPhysicalModifiers { get; private set; }
    private List<CatPhysicalModifier> _physicalModifiers = new List<CatPhysicalModifier>();
    [field: SerializeField] public List<CatPhysicalModifier> InvalidPhysicalModifiers { get; private set; }
    [field: SerializeField] public List<CatPhysicalModifier> ValidPhysicalModifiers { get; private set; }
    
    [Range(0, 1)]
    [SerializeField] private float validPhysicalModifierSkipChance;
    [SerializeField] private int maxValidPhysicalModifiers;
    [Range(0, 1)]
    [SerializeField] private float invalidPhysicalModifierSkipChance;

    // Manager
    public static ManagerCatModifier instance { get; private set; }

    public static Action OnPopulate;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Manager in the scene.");
        }
        instance = this;
    }

    private void OnEnable()
    {
        ManagerGame.OnGameStart += OnGameInitialize;
    }

    private void OnDisable()
    {
        ManagerGame.OnGameStart -= OnGameInitialize;
    }

    private void OnGameInitialize()
    {
        GeneratePhysicalModifiers();
        OnPopulate.Invoke();
    }

    private void OnDestroy()
    {
        _physicalModifiers.Clear();
        DefaultPhysicalModifiers.Clear();
        InvalidPhysicalModifiers.Clear();
        ValidPhysicalModifiers.Clear();
    }

    private void SetDifficulty()
    {
        Difficulty difficulty = ManagerGame.Instance.Difficulty;
        invalidPhysicalModifiersPoolCount = difficulty.invalidCount;
        validPhysicalModifierSkipChance = difficulty.validSkipChance;
        maxValidPhysicalModifiers = difficulty.maxValids;
        invalidPhysicalModifierSkipChance = difficulty.invalidSkipChance;
    }

    // Calls every process
    [ContextMenu("Populate")]
    public void GeneratePhysicalModifiers()
    {
        SetDifficulty();
        DefaultPhysicalModifiers = Resources.LoadAll<CatPhysicalModifier>(defaultPhysicalModifiersFilePath).ToList();
        _physicalModifiers = GetPhysicalModifiers();
        InvalidPhysicalModifiers = GetRandomPhysicalModifiers(invalidPhysicalModifiersPoolCount, _physicalModifiers);
        ValidPhysicalModifiers = GetRandomPhysicalModifiers(validPhysicalModifiersPoolCount, _physicalModifiers);
    }
    
    private List<CatPhysicalModifier> GetPhysicalModifiers()
    {
        List<CatPhysicalModifier> loaded = Resources.LoadAll<CatPhysicalModifier>(physicalModifiersFilePath).ToList();
        
        foreach (CatPhysicalModifier physicalModifier in loaded)
        {
            foreach (CatPhysicalModifier dependency in physicalModifier.Dependencies)
            {
                dependency.isDependency = true;
                dependency.dependencyParent = physicalModifier;
            }
        }
        
        // Removes dependencies
        for (int i = loaded.Count - 1; i >= 0; i--)
        {
            if (loaded[i].isDependency)
            {
                loaded.RemoveAt(i);
            }
        }
        
        return loaded;
    }
    
    private List<CatPhysicalModifier> GetRandomPhysicalModifiers(int count, List<CatPhysicalModifier> physicalModifiers)
    {
        List<CatPhysicalModifier> valid = new List<CatPhysicalModifier>();
        
        count = Mathf.Clamp(count, 0, physicalModifiers.Count);
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, physicalModifiers.Count);
            valid.Add(physicalModifiers[index]);
            physicalModifiers.RemoveAt(index);
        }
        
        return valid;
    }

    public void GenerateValidPhysicalModifiers(CatPhysicalModifierHelper helper)
    {
        List<CatPhysicalModifier> available = new List<CatPhysicalModifier>(ValidPhysicalModifiers);
        
        available = PrunePhysicalModifiers(helper, available);
        int count = validPhysicalModifiersCount;
        count = Mathf.Clamp(count, 0, available.Count);
        count = Mathf.Clamp(count, 0, helper.bodyParts.Count);
        count = Mathf.Clamp(count, 0, maxValidPhysicalModifiers);

        while (count > 0 && available.Count > 0)
        {
            count--;
            
            // if mimic true, then never skip, unless already is mimic
            if (Random.value > validPhysicalModifierSkipChance)
            {
                CatPhysicalModifier pm = available[Random.Range(0, available.Count)];

                ApplyPhysicalModifier(pm, helper);

                // Apply any dependent modifiers
                foreach (CatPhysicalModifier dependent in pm.Dependencies)
                {
                    ApplyPhysicalModifier(dependent, helper);
                }

                available = PrunePhysicalModifiers(helper, available);
            }
        }
        helper.UpdateModifierCount();
    }
    
    public void ResetPhysicalModifiers(CatPhysicalModifierHelper helper)
    {
        for (int i = 0; i < helper.bodyParts.Count; i++)
        {
            var part = helper.bodyParts[i];
            part.isOccupied = false;

            // Restore default mesh
            if (DefaultPhysicalModifiers.Count > i)
            {
                CatPhysicalModifier defaultCPM = DefaultPhysicalModifiers[i];
                part.smr.sharedMesh = defaultCPM.PhysicalModifier;
            }

            // Clear the reference to the applied modifier
            part.catPhysicalModifier = null;
        }

        // Make sure helper recomputes its internal count
        helper.UpdateModifierCount();
    }
    
    public void GenerateInvalidPhysicalModifiers(CatPhysicalModifierHelper helper)
    {
        List<CatPhysicalModifier> available = new List<CatPhysicalModifier>(InvalidPhysicalModifiers);
        
        available = PrunePhysicalModifiers(helper, available);
        int count = invalidPhysicalModifiersCount;
        count = Mathf.Clamp(count, 0, available.Count);
        count = Mathf.Clamp(count, 0, helper.bodyParts.Count);

        while (count > 0 && available.Count > 0)
        {
            count--;
            
            if (Random.value > invalidPhysicalModifierSkipChance || helper.ModifierCount == 0) // Negates modifying physicalModifierSkipChance % of the time, given that its 0-1
            {
                CatPhysicalModifier pm = available[Random.Range(0, available.Count)];

                ApplyPhysicalModifier(pm, helper);
                
                // Apply any dependent modifiers
                foreach (CatPhysicalModifier dependent in pm.Dependencies)
                {
                    ApplyPhysicalModifier(dependent, helper);
                }
                
                available = PrunePhysicalModifiers(helper, available);
                helper.UpdateModifierCount();
            }
        }
        helper.UpdateModifierCount();
    }

    // Helper function to avoid repeated code
    private void ApplyPhysicalModifier(CatPhysicalModifier pm, CatPhysicalModifierHelper helper)
    {
        for (int i = 0; i < helper.bodyParts.Count; i++)
        {
            if ((pm.OccupationFlags & (OccupationFlags)(1 << helper.bodyParts[i].id)) != 0)
            {
                helper.bodyParts[i].isOccupied = true;
                helper.bodyParts[i].smr.sharedMesh = pm.PhysicalModifier;
                helper.bodyParts[i].catPhysicalModifier = pm;
            }
        }
    }

    public void ForceAddInvalidPhysicalModifiers(CatPhysicalModifierHelper helper)
    {
        List<CatPhysicalModifier> available = new List<CatPhysicalModifier>(InvalidPhysicalModifiers);
        
        int count = invalidPhysicalModifiersCount;
        count = Mathf.Clamp(count, 0, available.Count);
        
        while (count > 0 && available.Count > 0)
        {
            CatPhysicalModifier pm = available[Random.Range(0, available.Count)];

            //print(pm.name);

            // Get current PM based on occupation of our randomly selected PM
            CatPhysicalModifier currentPm = ScriptableObject.CreateInstance<CatPhysicalModifier>();
            for (int i = 0; i < helper.bodyParts.Count; i++)
            {
                if ((pm.OccupationFlags & (OccupationFlags)(1 << helper.bodyParts[i].id)) != 0)
                {
                    currentPm = helper.bodyParts[i].catPhysicalModifier;
                    
                    // If dependency of parent, change to parent
                    if (currentPm.isDependency)
                    {
                        currentPm = currentPm.dependencyParent;
                        
                        // Since changed, default this too
                        for (int j = 0; j < helper.bodyParts.Count; j++)
                        {
                            if ((currentPm.OccupationFlags & (OccupationFlags)(1 << helper.bodyParts[j].id)) != 0)
                            {
                                helper.bodyParts[j].smr.sharedMesh = DefaultPhysicalModifiers[j].PhysicalModifier;
                            }
                        }
                    }
                    break;
                }
            }

            //print(currentPm.name);
            
            // Defaults any dependencies so that force add invalids doesn't interfere with them, (e.g. eyes only replacing part of Voxel)
            foreach (CatPhysicalModifier dependency in currentPm.Dependencies)
            {
                for (int i = 0; i < helper.bodyParts.Count; i++)
                {
                    if ((dependency.OccupationFlags & (OccupationFlags)(1 << helper.bodyParts[i].id)) != 0)
                    {
                        helper.bodyParts[i].smr.sharedMesh = DefaultPhysicalModifiers[i].PhysicalModifier;
                    }
                }
            }
            
            // Now apply modifiers
            ApplyPhysicalModifier(pm, helper);
            
            // Apply any dependent modifiers
            foreach (CatPhysicalModifier dependent in pm.Dependencies)
            {
                ApplyPhysicalModifier(dependent, helper);
            }
            
            available = PrunePhysicalModifiers(helper, available);
            helper.UpdateModifierCount();
            count--;
        }
        helper.UpdateModifierCount();
    }

    private List<CatPhysicalModifier> PrunePhysicalModifiers(CatPhysicalModifierHelper helper, List<CatPhysicalModifier> physicalModifiers)
    {
        List<CatPhysicalModifier> available = new List<CatPhysicalModifier>();
        foreach (CatPhysicalModifier pm in physicalModifiers)
        {
            if (CheckValidPhysicalModifier(pm, helper))
            {
                available.Add(pm);
            }
        }
        
        return available;
    }

    private bool CheckValidPhysicalModifier(CatPhysicalModifier pm, CatPhysicalModifierHelper helper)
    {
        // Currently nothing is occupied, since bitmask is 0
        OccupationFlags occupiedSpaces = 0;
        
        // Set physical modifier occupation id to spaces
        occupiedSpaces |= pm.OccupationFlags;

        // Inherit dependencies traits
        foreach (CatPhysicalModifier dependent in pm.Dependencies)
        {
            occupiedSpaces |= dependent.OccupationFlags;
        }
        
        // Detect if space already occupied
        for (int i = 0; i < helper.bodyParts.Count; i++)
        {
            if (helper.bodyParts[i].isOccupied)
            {
                // Check if the corresponding bit is already set in the bitmask
                if ((occupiedSpaces & (OccupationFlags)(1 << helper.bodyParts[i].id)) != 0)
                {
                    return false; // The space is occupied
                }
            }
        }

        return true;
    }

    [ContextMenu("Dev - Decorate All")]
    private void DevDecorateAll()
    {
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            CatDecorationHelper helper = obj.GetComponent<CatDecorationHelper>();
            
            if (helper != null)
            {
                //ManagerCatModifier.instance.GenerateDecorations(helper, 1);
            }
        }
    }
    
    [ContextMenu("Dev - Physically Modify All")]
    private void DevPhysicallyModifyAll()
    {
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            CatPhysicalModifierHelper helper = obj.GetComponent<CatPhysicalModifierHelper>();
            
            if (helper != null)
            {
                GenerateValidPhysicalModifiers(helper);
            }
        }
    }
}
