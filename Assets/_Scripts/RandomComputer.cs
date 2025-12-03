using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomComputer : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer smr;
    [SerializeField] private string computerPath = "Models/Computers/Variants"; // The folder path within the Resources folder.
    [SerializeField] private List<Material> computerMaterials;

    private void Start()
    {
        LoadComputers();
        RandomizeComputer();
    }

    private void LoadComputers()
    {
        Material[] loadedMats = Resources.LoadAll<Material>(computerPath);

        foreach (Material mat in loadedMats)
        {
            computerMaterials.Add(mat);
        }
    }

    [ContextMenu("Generate")]
    public void RandomizeComputer()
    {
        smr.material = computerMaterials[Random.Range(0, computerMaterials.Count)];
    }
}
