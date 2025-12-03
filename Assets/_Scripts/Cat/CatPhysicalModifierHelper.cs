using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CatPhysicalModifierHelper : MonoBehaviour
{
    [Header("Physical Modifiers")]
    public List<BodyPart> bodyParts;

    [Serializable]
    public class BodyPart
    {
        public int id;
        public SkinnedMeshRenderer smr;
        public CatPhysicalModifier catPhysicalModifier;
        public bool isOccupied;
    }
    
    [Header("References")]
    public bool isMimic;
    public bool isProcessing;

    [field: SerializeField] public int ModifierCount { get; private set; }
    [SerializeField] private bool isTestCasePassed;
    
    private void Start()
    {
        if (!isProcessing) return;
        
        if (isMimic)
        {
            GenerateInvalidPhysicalModifiers();
            isTestCasePassed = CopyCatCheckTestCase();
        }
        else
        {
            GenerateValidPhysicalModifiers();
        }
    }
    
    public void ResetModifierCount()
    {
        ModifierCount = 0;
    }

    public void UpdateModifierCount()
    {
        List<CatPhysicalModifier> invalidModifiers = ManagerCatModifier.instance.InvalidPhysicalModifiers;

        // Extract only valid sharedMeshes (filtering out null smr values)
        HashSet<Mesh> meshes = new HashSet<Mesh>(
            bodyParts.Where(bodyPart => bodyPart.smr != null)
                .Select(bodyPart => bodyPart.smr.sharedMesh)
        );

        // Count how many invalidModifiers have a PhysicalModifier matching the meshes
        ModifierCount = invalidModifiers.Count(modifier => meshes.Contains(modifier.PhysicalModifier));
    }

    private bool CopyCatCheckTestCase()
    {
        List<CatPhysicalModifier> invalids = new List<CatPhysicalModifier>(ManagerCatModifier.instance.InvalidPhysicalModifiers);

        // Collect all shared meshes from bodyParts
        List<Mesh> sharedMeshes = bodyParts.Select(bp => bp.smr.sharedMesh).ToList();

        // Check if any invalid modifier matches any shared mesh
        return invalids.Any(pm => sharedMeshes.Contains(pm.PhysicalModifier));
    }

    [ContextMenu("Reset")]
    public void ResetPhysicalModifiers()
    {
        ManagerCatModifier.instance.ResetPhysicalModifiers(this);
    }
    
    [ContextMenu("Refresh")]
    public void RefreshPhysicalModifiers()
    {
        ManagerCatModifier.instance.ResetPhysicalModifiers(this);
        if (isMimic)
        {
            GenerateInvalidPhysicalModifiers();
        }
        else
        {
            GenerateValidPhysicalModifiers();
        }
    }
    
    [ContextMenu("Valid")]
    public void GenerateValidPhysicalModifiers()
    {
        ManagerCatModifier.instance.GenerateValidPhysicalModifiers(this);
    }
    
    [ContextMenu("Invalid")]
    public void GenerateInvalidPhysicalModifiers()
    {
        ManagerCatModifier.instance.GenerateInvalidPhysicalModifiers(this);
        ManagerCatModifier.instance.GenerateValidPhysicalModifiers(this);
    }

    [ContextMenu("Force Invalid")]
    public void ForceAddInvalidPhysicalModifiers()
    {
        ManagerCatModifier.instance.ForceAddInvalidPhysicalModifiers(this);
        ManagerCatModifier.instance.GenerateValidPhysicalModifiers(this);
        isMimic = true;
        isTestCasePassed = CopyCatCheckTestCase();
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            if (isTestCasePassed && isMimic)
            {
                Gizmos.color = Color.green;
            }
            else if (isMimic)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.white;
            }
            Gizmos.DrawSphere(transform.position + new Vector3(-0.2f, 1f, 0), 0.2f);
            DrawStringGizmo.DrawString(ModifierCount.ToString(), transform.position + new Vector3(0.2f, 1f, 0), Gizmos.color, new Vector2(0.5f, 0.5f), 10f);
        }
    }
}

