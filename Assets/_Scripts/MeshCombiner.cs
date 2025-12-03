using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    //public MeshRenderer parentMeshRenderer; // Reference to the parent MeshRenderer
    
    void Start()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);
        transform.GetComponent<MeshFilter>().sharedMesh = mesh;
        transform.gameObject.SetActive(true);
    }
    

    /*
    void CombineChildMeshRenderers()
    {
        // Get all MeshRenderers in the children of this GameObject
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();

        // Create a list to store the meshes of all child objects
        List<CombineInstance> combineInstances = new List<CombineInstance>();

        // Loop through each MeshRenderer and add its mesh to the combineInstances list
        foreach (MeshRenderer renderer in meshRenderers)
        {
            // Get the MeshFilter attached to the same object
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();

            // Add the mesh to the combineInstances list
            CombineInstance combineInstance = new CombineInstance
            {
                mesh = meshFilter.sharedMesh,
                transform = renderer.transform.localToWorldMatrix * renderer.transform.parent.worldToLocalMatrix
            };

            // Add the combineInstance to the list
            combineInstances.Add(combineInstance);

            // Disable the MeshRenderer to hide the child mesh
            renderer.enabled = false;
        }

        // Create a new mesh for the parent object
        Mesh combinedMesh = new Mesh();

        // Combine the meshes
        combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);

        // Ensure the parent GameObject has a MeshFilter component and assign the combined mesh
        MeshFilter parentMeshFilter = parentMeshRenderer.GetComponent<MeshFilter>();
        if (parentMeshFilter == null)
        {
            parentMeshFilter = parentMeshRenderer.gameObject.AddComponent<MeshFilter>();
        }

        // Assign the combined mesh to the MeshFilter
        parentMeshFilter.sharedMesh = combinedMesh;

        // Enable the parent MeshRenderer
        parentMeshRenderer.enabled = true;
    }
    */



}
