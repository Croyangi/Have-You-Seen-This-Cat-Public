using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;

public class MetalDoor : MonoBehaviour
{
    [SerializeField] private GameObject left;
    [SerializeField] private GameObject right;
    
    [SerializeField] private float openAmount;
    [field: SerializeField] public bool IsOpen { get; private set; }
    
    [SerializeField] private float doorCloseTimer;
    [SerializeField] private BoxCollider graphUpdateBounds;
    [SerializeField] private GameObject[] onOpenedObjs;
    
    [SerializeField] private AudioClip open;
    [SerializeField] private AudioClip close;
    [SerializeField] private AudioClip steam;

    [SerializeField] private Transform audioSpot;

    private Coroutine _autoCloseDoor;

    public Action OnDoorOpen;
    public Action OnDoorClose;
    

    [ContextMenu("Open")]
    public void Open()
    {
        SFXObject temp = ManagerSFX.Instance.PlaySFX(steam, audioSpot.position, 0.1f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        ManagerSFX.Instance.ApplyLowPassFilter(temp);
        
        StartCoroutine(OpenDoor());
    }

    private IEnumerator OpenDoor()
    {
        yield return new WaitForSeconds(0.3f);
        
        SFXObject temp = ManagerSFX.Instance.PlaySFX(open, audioSpot.position, 0.1f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        ManagerSFX.Instance.ApplyLowPassFilter(temp);
        
        float duration = 0.7f;
        
        IsOpen = true;
        left.transform.DOLocalMoveX(-openAmount, duration).SetEase(Ease.OutBounce);
        right.transform.DOLocalMoveX(openAmount, duration).SetEase(Ease.OutBounce);

        foreach (GameObject obj in onOpenedObjs)
        {
            obj.SetActive(false);
        }
        
        //if (_autoCloseDoor != null) { StopCoroutine(_autoCloseDoor); }
        //_autoCloseDoor = StartCoroutine(AutoCloseTimer(doorCloseTimer));
        StartCoroutine(ApplyGraphUpdate(duration));
        
        OnDoorOpen?.Invoke();
    }

    private IEnumerator ApplyGraphUpdate(float time)
    {
        yield return new WaitForSeconds(time);

        var guo = new GraphUpdateObject(graphUpdateBounds.bounds) {
            updatePhysics = true,
            modifyWalkability = false,
            modifyTag = false,
        };
        
        AstarPath.active.UpdateGraphs(guo);
    }
    
    [ContextMenu("Close")]
    public void Close()
    {
        CloseDoor();
        
        if (_autoCloseDoor != null) { StopCoroutine(_autoCloseDoor); }
        StartCoroutine(ApplyGraphUpdate(0.5f));
    }

    private void CloseDoor()
    {
        float duration = 0.5f;
        IsOpen = false;
        left.transform.DOLocalMoveX(0, duration).SetEase(Ease.OutBounce);
        right.transform.DOLocalMoveX(0, duration).SetEase(Ease.OutBounce);
        
        SFXObject temp = ManagerSFX.Instance.PlaySFX(close, audioSpot.position, 0.1f, false, ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        ManagerSFX.Instance.ApplyLowPassFilter(temp);
        
        foreach (GameObject obj in onOpenedObjs)
        {
            obj.SetActive(true);
        }
        
        OnDoorClose?.Invoke();
    }

    private IEnumerator AutoCloseTimer(float time)
    {
        yield return new WaitForSeconds(time);
        CloseDoor();
    }
}
