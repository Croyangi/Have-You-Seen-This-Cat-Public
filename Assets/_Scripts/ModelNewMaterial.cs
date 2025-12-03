using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelNewMaterial : MonoBehaviour
{
    private void Awake()
    {
        List<SkinnedMeshRenderer> smrs = new List<SkinnedMeshRenderer>();
        smrs.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>());
        foreach (SkinnedMeshRenderer smr in smrs)
        {
            Material[] originalMats = smr.sharedMaterials;
            Material[] newMats = new Material[originalMats.Length];
            for (int i = 0; i < originalMats.Length; i++)
            {
                newMats[i] = new Material(originalMats[i]);
            }
            smr.materials = newMats;
        }

    }
}
