using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Serializable] 
    public class Doorway
    {
        public Transform pos;
        public bool isOpen;
        public GameObject doorFiller;
    }
    
    public List<Doorway> doorways;
    public BoxCollider[] boundingBoxes;

    public void OpenDoorway(Doorway doorway)
    {
        doorway.isOpen = true;
        doorway.doorFiller.SetActive(false);
    }

    public void CloseDoorway(Doorway doorway)
    {
        doorway.isOpen = false;
        doorway.doorFiller.SetActive(true);
    }

}
