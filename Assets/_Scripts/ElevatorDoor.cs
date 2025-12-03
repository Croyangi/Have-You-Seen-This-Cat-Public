using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ElevatorDoor : MonoBehaviour
{
    [SerializeField] private GameObject gate;
    [SerializeField] private bool isDoorOpen;
    [SerializeField] private GameObject gateHitbox;
    
    
    [ContextMenu("Open")]
    public void Open()
    {
        if (_openElevator != null) StopCoroutine(_openElevator);
        if (_handleClosing != null) StopCoroutine(_handleClosing);
        _openElevator = StartCoroutine(HandleOpening());
    }

    [ContextMenu("Close")]
    public void Close()
    {
        if (_openElevator != null) StopCoroutine(_openElevator);
        if (_handleClosing != null) StopCoroutine(_handleClosing);
        _handleClosing = StartCoroutine(HandleClosing());
    }

    private Coroutine _openElevator;
    private float _gateOpenPosition = 2.2f;
    private float _gateClosePosition = -0.4f;
    [SerializeField] private AudioClip gateOpenSFX;
    [SerializeField] private AudioClip gateCloseSFX;
    [SerializeField] private Transform audioSpot;
    private IEnumerator HandleOpening()
    {
        gateHitbox.SetActive(false);
        gate.transform.DOLocalMoveY(_gateClosePosition, 0f);
        gate.transform.DOComplete();
        
        ManagerSFX.Instance.PlaySFX(gateOpenSFX, audioSpot.position, 0.1f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, parent: audioSpot);
        
        gate.transform.DOLocalMoveY(gate.transform.localPosition.y + 0.2f, 0.6f).SetEase(Ease.OutBounce);
        yield return new WaitForSeconds(1f);

        float duration = 2.8f;
        gate.transform.DOLocalMoveY(_gateOpenPosition, duration).SetEase(Ease.InOutCubic);
        yield return new WaitForSeconds(duration);

        isDoorOpen = true;
    }

    private Coroutine _handleClosing;
    private IEnumerator HandleClosing()
    {
        gateHitbox.SetActive(true);
        gate.transform.DOLocalMoveY(_gateOpenPosition, 0f);
        gate.transform.DOComplete();
        
        ManagerSFX.Instance.PlaySFX(gateCloseSFX, audioSpot.position, 0.1f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, parent: audioSpot);
        
        float duration1 = 1f;
        gate.transform.DOLocalMoveY(_gateClosePosition, duration1).SetEase(Ease.InCubic).SetEase(Ease.OutBounce);
        yield return new WaitForSeconds(duration1);

        isDoorOpen = false;
    }
}
