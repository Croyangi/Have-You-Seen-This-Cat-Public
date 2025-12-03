using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProcessable
{
    public bool IsProcessing { get; set; }

    public virtual void OnProcessingChanged()
    {
        
    }
}