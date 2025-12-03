using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryHelper : MonoBehaviour
{
    [SerializeField] private PlayerInteractHelper interactHelper;
    [SerializeField] private PlayerInspectingHelper inspectingHelper;
    
    [field: SerializeField] public InventoryItem[] Inventory { get; private set; }
    [field: SerializeField] public InventoryItem CurrentItem { get; private set; }
    [SerializeField] private int inventoryIndex;

    public GameObject heldItem;
    public bool isForceHold;

    public void Awake()
    {
        Inventory = new InventoryItem[3];
    }

    public bool IsAvailableSpace()
    {
        foreach (InventoryItem item in Inventory)
        {
            if (item == null)
            {
                return true;
            }
        }

        return false;
    }

    public void AddItem(InventoryItem item)
    {
        if (!IsAvailableSpace()) return;

        Inventory[SetAvailableSpace()] = item;
    }

    public void RemoveItem(InventoryItem item)
    {
        foreach (InventoryItem i in Inventory)
        {
            if (item == i)
            {
                Inventory[SetAvailableSpace()] = null;
            }
        }
    }

    public void DropHeldItem()
    {
        inspectingHelper.OnEndInspection();
    }
    
    public void RemoveHeldItem()
    {
        Destroy(heldItem);
        inspectingHelper.OnEndInspection();
    }

    public void DropItem()
    {
        
    }

    private int SetAvailableSpace()
    {
        for (int i = 0; i < Inventory.Length; i++)
        {
            if (Inventory[i] == null)
            {
                inventoryIndex = i;
            }
        }

        Debug.LogWarning("No available space.");
        return 0;
    }
}
