using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPoster : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer smr;
    [SerializeField] private string posterPath = "Models/Posters/Variants"; // The folder path within the Resources folder.
    [SerializeField] private List<Material> posterMaterials;

    private void Start()
    {
        LoadPosters();
        RandomizePoster();
    }

    private void LoadPosters()
    {
        Material[] loadedMats = Resources.LoadAll<Material>(posterPath);

        foreach (Material mat in loadedMats)
        {
            posterMaterials.Add(mat);
        }
    }

    [ContextMenu("Generate")]
    public void RandomizePoster()
    {
        smr.material = posterMaterials[Random.Range(0, posterMaterials.Count)];
    }
}
