using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollCallLeaveDetect : MonoBehaviour
{
    private void OnTriggerExit(Collider collider)
    {
        ManagerCat.instance.OnLeaveRollCallArea();
    }
}
