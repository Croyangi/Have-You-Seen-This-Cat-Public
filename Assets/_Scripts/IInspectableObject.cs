using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInspectableObject
{
    public void OnStartInspect();

    public void OnEndInspect();
}
