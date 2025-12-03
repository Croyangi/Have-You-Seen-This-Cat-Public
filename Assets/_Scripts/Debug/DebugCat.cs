using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCat : MonoBehaviour, IDebugCommandSource
{
    [SerializeField] private GameObject cat;
    [SerializeField] private DebugCatXRay xRay;
    
    public IEnumerable<DebugCommand> GetCommands()
    {
        yield return new DebugCommand(
            "cat_spawn",
            "Spawns X amount of cats.",
            "cat_spawn <count>",
            args =>
            {
                int count = 1;

                if (args.Length > 0)
                {
                    if (!int.TryParse(args[0], out count) || count < 1)
                    {
                        Debug.LogWarning("Invalid amount.");
                        return;
                    }
                }
                
                SpawnCat(count);
            }
            
        );
        
        yield return new DebugCommand(
            "cat_find",
            "Finds all existing cats.",
            "cat_find",
            args =>
            {
                FindAllCats();
            }
            
        );
        
        yield return new DebugCommand(
            "cat_speedy",
            "Speeds up all cats, for your entertainment.",
            "cat_speedy",
            args =>
            {
                SpeedyCats();
            }
            
        );
        
        yield return new DebugCommand(
            "cat_radar",
            "Toggles Cat radar.",
            "cat_radar",
            args =>
            {

                CatRadar();
            }
            
        );
    }
    
    private void CatRadar()
    {
        xRay.OnTogglePerformed();
    }
    
    private void SpawnCat(int count = 1)
    {
        GameObject playerHead = ManagerPlayer.instance.PlayerHead; 
        Vector3 spawnPos = playerHead.transform.position;
        Ray ray = new Ray(playerHead.transform.position, playerHead.transform.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 30, LayerUtility.Environment))
        {
            spawnPos = hit.point;
        }
        
        for (int i = 0; i < count; i++)
        {
            GameObject spawnedCat = Instantiate(cat, spawnPos, Quaternion.identity);
            spawnedCat.GetComponent<Cat>().OnInteract();
        }
    }
    
    private void FindAllCats()
    {
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        // Loop through each object
        foreach (GameObject obj in allObjects)
        {
            // Check if the object has the "Cat" script attached
            Cat catScript = obj.GetComponent<Cat>();
            
            // If the script is found, call OnInteract
            if (catScript != null)
            {
                catScript.OnInteract();
            }
        }
    }
    
    private void SpeedyCats()
    {
        foreach (CatMovementHelper helper in ManagerCat.instance.CatsMovementHelpers)
        {
            helper.SetMovementSpeed(20);
        }
    }
}
