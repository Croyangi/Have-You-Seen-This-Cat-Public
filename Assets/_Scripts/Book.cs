using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Book : InteractableObject, IInspectableObject
{
    [SerializeField] private Renderer myRenderer;
    private MaterialPropertyBlock _randomColorMPB;
    
    [SerializeField] private string bookTextsFile = "BookTexts.txt";
    
    public override void OnInteract()
    {
        
    }

    protected override void Start()
    {
        base.Start();
        
        _randomColorMPB = new MaterialPropertyBlock();
        myRenderer.GetPropertyBlock(_randomColorMPB, 0);
        _randomColorMPB.SetColor("_BaseColor", Random.ColorHSV());
        myRenderer.SetPropertyBlock(_randomColorMPB, 0);
        
        InspectText = "\"" + TextFileReader.GetRandomLine(Application.streamingAssetsPath, bookTextsFile) + "\"";
    }

    public override void ShowOutline()
    {
        mpb.Clear();
        myRenderer.GetPropertyBlock(mpb, 1);
        mpb.SetColor("_OutlineColor", selectedOutlineColor);
        myRenderer.SetPropertyBlock(mpb, 1);
    }

    public override void HideOutline()
    {
        mpb.Clear();
        myRenderer.GetPropertyBlock(mpb, 1);
        mpb.SetColor("_OutlineColor", originalOutlineColor);
        myRenderer.SetPropertyBlock(mpb, 1);
    }
    
    public void OnStartInspect()
    {
        IsBeingInteracted = true;
    }

    public void OnEndInspect()
    {
        IsBeingInteracted = false;
    }
}
