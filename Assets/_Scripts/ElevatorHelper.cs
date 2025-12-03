using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ElevatorHelper : MonoBehaviour, IDataPersistence
{
    [SerializeField] private GameObject[] shaftPieces;
    [SerializeField] private float shaftPieceMaxYPos;
    [SerializeField] private float shaftPieceHeight;
    [SerializeField] private float elevatorShaftFallSpeed;
    [field: SerializeField] public GameObject Elevator { get; private set; }
    [SerializeField] private Rigidbody rb;

    [SerializeField] private GameObject player;
    
    [SerializeField] private float elevatorDescendToProcessingTime;
    [SerializeField] private float elevatorDescendToGameplayTime;
    [SerializeField] private Transform elevatorFloor;
    [SerializeField] private ParticleSystem elevatorParticles;

    [SerializeField] private Transform[] stoppingPoints; // 0 waiting room, 1 processing, 2 gameplay
    
    [SerializeField] private Floor[] floors;
    [SerializeField] private int floorIndex;

    [SerializeField] private bool hasProcessed; // Controls elevator shaft moving animation
    [field: SerializeField] public bool IsInAction { get; private set; }
    [field: SerializeField] public bool HasTakenElevator { get; private set; }
    
    [SerializeField] private AudioClip elevatorProcessingSFX;
    [SerializeField] private AudioClip elevatorKickstartSFX;
    [SerializeField] private AudioClip elevatorDingSFX;
    [SerializeField] private AudioClip elevatorPrepareHighGearSFX;
    [SerializeField] private AudioClip elevatorDescendSlowSFX;
    [SerializeField] private AudioClip elevatorStopSFX;
    [SerializeField] private AudioClip elevatorDescendToGameplaySFX;
    [SerializeField] private AudioClip floorTextRevealSFX;
    [SerializeField] private AudioClip caveAmbienceSFX;
    
    [SerializeField] private TextMeshProUGUI depthTextMesh;
    [SerializeField] private TextMeshProUGUI nameTextMesh;
    [SerializeField] private Image depthBacking;
    [SerializeField] private Image nameBacking;

    [field: SerializeField] public List<GameObject> CatObjs { get; private set; }

    private AudioMixerGroup _sfxMixer;
    
    public void LoadData(GameData data)
    {
        floorIndex = data.expedition.floor;
        
        if (data.hasTakenElevator && SceneManager.GetActiveScene().name == SceneID.Gameplay.ToString())
        {
            if (floorIndex == 0) floorIndex = 1;
            if (floorIndex > 3)
            {
                data.expedition.isOngoing = false;
                SceneLoader.Load(SceneID.DemoEnd);
                return;
            }
            StartCoroutine(GameplayFloorArrival());
        }
    }
    
    public void SaveData(ref GameData data)
    {
        data.hasTakenElevator = HasTakenElevator;
        if (SceneManager.GetActiveScene().name == SceneID.Gameplay.ToString())
        {
            data.expedition.floor = floorIndex;
            if (data.expedition.floor >= 2)
            {
                data.expedition.isOngoing = true;
            }
        }
    }

    public Floor GetFloor()
    {
        return floors[Math.Clamp(floorIndex, 1, floors.Length - 1)];
    }
    
    private void Start()
    {
        floorIndex %= floors.Length;
        
        _sfxMixer = ManagerAudioMixer.Instance.AMGSFX;
        player = ManagerPlayer.instance.PlayerObj;
        Elevator.transform.localPosition = new Vector3(Elevator.transform.localPosition.x, stoppingPoints[0].transform.localPosition.y, Elevator.transform.localPosition.z);

        depthBacking.gameObject.SetActive(false);
        nameBacking.gameObject.SetActive(false);
    }

    public void AddCatToElevator(GameObject cat)
    {
        CatObjs.Add(cat);
        cat.transform.parent = Elevator.transform;
    }

    private void ResetElevator()
    {
        floorIndex = 0;
        Elevator.transform.DOMoveY(stoppingPoints[0].position.y, 0f);
        hasProcessed = false;
        IsInAction = false;
        HasTakenElevator = false;
    }

    private void PrepareElevatorDescent()
    {
        if (IsInAction) return;
        IsInAction = true;
        HasTakenElevator = true;
        
        ManagerSFX.Instance.StopAmbienceSFX();
        ManagerSFX.Instance.PlaySFX(elevatorDingSFX, Elevator.transform.position, 0.1f, false, _sfxMixer);
        ManagerAmbience.Instance.IsParticlesFollowing = false;
    }
    
    private IEnumerator GameplayFloorArrival()
    {
        PrepareElevatorDescent();
        
        yield return new WaitForFixedUpdate();
        PrepareGameplayDescent();
        
        StartCoroutine(HandleGameplayFloorArrival());
    }

    private void PrepareGameplayDescent()
    {
        ManagerPlayer mp = ManagerPlayer.instance;
        mp.PlayerHead.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        mp.PlayerVFXHelper.PlayerFadeToAwake();
    }

    public void ContinueExpedition()
    {
        PrepareElevatorDescent();
        floorIndex++;
        StartCoroutine(HandleContinueExpedition());
    }

    private IEnumerator HandleContinueExpedition()
    {
        yield return ElevatorDescendUpdate(stoppingPoints[3].transform);
        
        yield return new WaitForSeconds(3f);
        float duration = 4f;
        ManagerPlayer.instance.PlayerVFXHelper.PlayerFadeToBlack(duration);
        yield return new WaitForSeconds(duration);
        ManagerDataPersistence.Instance.SaveGame();
        SceneLoader.Load(SceneID.Gameplay);
    }

    public void WaitingRoomDescent(bool isLoadingSave)
    {
        PrepareElevatorDescent();
        floorIndex = isLoadingSave ? Math.Clamp(floorIndex, 1, floors.Length - 1) : 0;
        StartCoroutine(HandleWaitingRoomDescent());
    }

    private IEnumerator HandleWaitingRoomDescent()
    {
        yield return ElevatorDescendUpdate(stoppingPoints[1].transform);
        
        yield return new WaitForSeconds(3f);
        float duration = 4f;
        ManagerPlayer.instance.PlayerVFXHelper.PlayerFadeToBlack(duration);
        yield return new WaitForSeconds(duration);
        ManagerDataPersistence.Instance.SaveGame();
        SceneLoader.Load(SceneID.Gameplay);
    }

    [ContextMenu("Force Descend To Gameplay")]
    public void ForceDescend()
    {
        if (IsInAction) return;
        IsInAction = true;
        
        ManagerSFX.Instance.StopAmbienceSFX();
        ManagerAmbience.Instance.IsParticlesFollowing = false;
        OnFloorName();
        StartCoroutine(OnFadeOutText());
        ManagerSFX.Instance.PlayRawSFX(floorTextRevealSFX, 0.2f, false, _sfxMixer);
        ManagerSFX.Instance.PlayAmbienceSFX(caveAmbienceSFX, 0.05f);
        
        StartCoroutine(ElevatorDescendUpdate(stoppingPoints[2].transform, true));
        
        ElevatorComplete();
    }

    [ContextMenu("Return to Surface")]
    public void ReturnToSurface()
    {
        if (IsInAction) return;
        IsInAction = true;
        floorIndex++;
        ManagerSFX.Instance.PlaySFX(elevatorDingSFX, Elevator.transform.position, 0.1f, false, _sfxMixer);
        hasProcessed = false;
        ManagerDataPersistence.Instance.SaveGame();
        StartCoroutine(HandleReturnToSurface());
    }

    [SerializeField] private float returnToSurfaceTransitTime;
    private IEnumerator HandleReturnToSurface()
    {
        _lockPlayerToElevator = StartCoroutine(LockPlayerToElevatorUpdate());
        
        yield return new WaitForSeconds(1f);

        // Kickstart bounce
        float duration0 = 1f;
        ManagerSFX.Instance.PlaySFX(elevatorKickstartSFX, Elevator.transform.position, 0.1f, false, _sfxMixer);
        Elevator.transform.DOMoveY(Elevator.transform.position.y - 0.1f, duration0).SetEase(Ease.OutBounce);

        yield return new WaitForSeconds(duration0);
        yield return new WaitForSeconds(1f);

        // Descend slow SFX
        GameObject descendSFXObj = ManagerSFX.Instance.PlaySFX(elevatorDescendSlowSFX, transform.position, 0.1f, false, _sfxMixer, parent: Elevator.transform).gameObject;
        descendSFXObj.transform.position = Elevator.transform.position;
        
        Elevator.transform.DOLocalMoveY(stoppingPoints[4].transform.localPosition.y, returnToSurfaceTransitTime).SetEase(Ease.InQuad);
        
        float duration = 4f;
        yield return new WaitForSeconds(returnToSurfaceTransitTime - duration);
        ManagerPlayer.instance.PlayerVFXHelper.PlayerFadeToBlack(duration);
        yield return new WaitForSeconds(duration);
        SceneLoader.Load(SceneID.WaitingRoom);
    }

    [ContextMenu("Deliver Cats")]
    public void DeliverCats()
    {
        if (IsInAction) return;
        IsInAction = true;
        
        ManagerSFX.Instance.PlaySFX(elevatorDingSFX, Elevator.transform.position, 0.1f, false, _sfxMixer);
        hasProcessed = false;
        StartCoroutine(HandleDeliverCats());
    }

    [SerializeField] private float deliverCatsTransitTime;
    [SerializeField] private float deliverCatsDeliveringTime;
    private IEnumerator HandleDeliverCats()
    {
        yield return new WaitForSeconds(2f);

        // Kickstart bounce
        float duration0 = 1f;
        ManagerSFX.Instance.PlaySFX(elevatorKickstartSFX, Elevator.transform.position, 0.1f, false, _sfxMixer);
        Elevator.transform.DOMoveY(Elevator.transform.position.y - 0.1f, duration0).SetEase(Ease.OutBounce);

        yield return new WaitForSeconds(duration0);
        yield return new WaitForSeconds(1f);

        // Descend slow SFX
        GameObject descendSFXObj = ManagerSFX.Instance.PlaySFX(elevatorDescendSlowSFX, transform.position, 0.1f, false, _sfxMixer, parent: Elevator.transform).gameObject;
        descendSFXObj.transform.position = Elevator.transform.position;
        
        Tween moveElevatorUp = Elevator.transform.DOLocalMoveY(stoppingPoints[4].transform.localPosition.y, deliverCatsTransitTime).SetEase(Ease.InQuad);
        yield return moveElevatorUp.WaitForCompletion();
        Destroy(descendSFXObj);
        
        yield return new WaitForSeconds(deliverCatsDeliveringTime);
        foreach (GameObject cat in CatObjs) Destroy(cat);
        CatObjs.Clear();
        
        // Descend slow SFX
        float offsetTime = elevatorDescendToGameplaySFX.length - elevatorDescendToGameplayTime;
        GameObject descendToGameplaySFXObj = ManagerSFX.Instance.PlaySFX(elevatorDescendToGameplaySFX, transform.position, 0.1f, false, _sfxMixer, parent: Elevator.transform).gameObject;
        
        float offset = 0.3f;
        descendToGameplaySFXObj.transform.position = Elevator.transform.position;
        Tween moveElevatorDown = Elevator.transform.DOLocalMoveY(stoppingPoints[2].transform.localPosition.y + offset, elevatorDescendToGameplayTime).SetEase(Ease.OutExpo);
        yield return moveElevatorDown.WaitForCompletion();
        yield return new WaitForSeconds(offsetTime);
        
        float duration1 = 1f;
        Elevator.transform.DOMoveY(Elevator.transform.position.y - offset, duration1).SetEase(Ease.OutBounce);
        
        ManagerSFX.Instance.PlaySFX(elevatorStopSFX, Elevator.transform.position, 0.1f, false, _sfxMixer);
        
        IsInAction = false;
    }

    [SerializeField] private AudioClip depthMeterCountSFX;
    [SerializeField] private float depthMeterCountSFXCooldown;
    private int _depthMeterCount;
    private IEnumerator OnDepthCounter(int depth, float time)
    {
        depth++;
        
        depthTextMesh.DOFade(0, 0);
        depthBacking.DOFade(0, 0);
        depthBacking.gameObject.SetActive(true);

        _depthMeterSFXUpdate = StartCoroutine(DepthMeterSFXUpdate());

        string suffix = "m";
        
        depthBacking.color = new Color(0, 0, 0, 0);
        depthTextMesh.text = "0" + suffix;
        depthTextMesh.color = new Color(depthTextMesh.color.r, depthTextMesh.color.g, depthTextMesh.color.b, 0f);

        int currentValue = 0;
        int lastValue = -1;
            
        Tween numberTween = DOTween.To(() => currentValue, x =>
        {
            currentValue = x;
            depthTextMesh.text = currentValue + suffix;
            _depthMeterCount = currentValue;
        }, depth, time).SetEase(Ease.OutExpo);
        
        depthBacking.DOFade(1f, time / 2).SetEase(Ease.InCubic);
        depthTextMesh.DOFade(1f, time).SetEase(Ease.InCubic);
        
        yield return numberTween.WaitForCompletion();
        
        StopCoroutine(_depthMeterSFXUpdate);
    }

    private Coroutine _depthMeterSFXUpdate;
    private IEnumerator DepthMeterSFXUpdate()
    {
        float timer = 0f;
        int lastValue = _depthMeterCount;
        int depth = floors[floorIndex].depth;

        while (true)
        {
            // Update timer
            if (timer > 0f)
                timer -= Time.deltaTime;

            // Check if value changed AND cooldown is ready
            if (_depthMeterCount != lastValue && timer <= 0f)
            {
                float volume = 0.05f * (_depthMeterCount / (float)depth);
                ManagerSFX.Instance.PlayRawSFX(depthMeterCountSFX, volume, false, _sfxMixer, 0.1f);

                timer = depthMeterCountSFXCooldown; // reset cooldown
            }

            lastValue = _depthMeterCount;

            yield return new WaitForFixedUpdate();
        }
    }
    
    private void OnFloorName()
    {
        string prefix = "<shake a=0.1>";
        string suffix = "</shake>";
        
        nameTextMesh.DOFade(1, 0);
        nameBacking.DOFade(1, 0);
        nameBacking.gameObject.SetActive(true);
        
        nameTextMesh.text = prefix + GetFloor().name + suffix;
        depthTextMesh.text = prefix + GetFloor().depth + "m" + suffix;
    }

    private IEnumerator OnFadeOutText()
    {
        yield return new WaitForSeconds(1f);

        float duration = 2f;
        
        Tween tween = depthTextMesh.DOFade(0, duration).SetEase(Ease.InSine);
        depthBacking.DOFade(0, duration).SetEase(Ease.InSine);
        nameTextMesh.DOFade(0, duration).SetEase(Ease.InSine);
        nameBacking.DOFade(0, duration).SetEase(Ease.InSine);
        
        yield return tween.WaitForCompletion();
        
        depthBacking.gameObject.SetActive(false);
        nameBacking.gameObject.SetActive(false);
    }
    
    [SerializeField] private NoiseSettings elevatorShakeNoise;
    private GameObject _processingSFXObj;
    private IEnumerator ElevatorDescendUpdate(Transform floor, bool force = false)
    {
        if (force)
        {
            Elevator.transform.localPosition = new Vector3(Elevator.transform.localPosition.x, floor.localPosition.y, Elevator.transform.localPosition.z);
            ManagerAmbience.Instance.IsParticlesFollowing = true;
            IsInAction = false;
            hasProcessed = false;
            yield break;
        }
        
        yield return new WaitForSeconds(2f);

        // Kickstart bounce
        float duration = 1f;
        ManagerSFX.Instance.PlaySFX(elevatorKickstartSFX, Elevator.transform.position, 0.1f, false, _sfxMixer);
        Elevator.transform.DOMoveY(Elevator.transform.position.y - 0.1f, duration).SetEase(Ease.OutBounce);

        yield return new WaitForSeconds(duration);
        yield return new WaitForSeconds(1f);

        // Descend slow SFX
        GameObject descendSFXObj = ManagerSFX.Instance.PlaySFX(elevatorDescendSlowSFX, transform.position, 0.1f, false, _sfxMixer, parent: Elevator.transform).gameObject;
        descendSFXObj.transform.position = Elevator.transform.position;

        // Start locking player + descent
        _lockPlayerToElevator = StartCoroutine(LockPlayerToElevatorUpdate());
        Tween moveElevator = Elevator.transform.DOMoveY(floor.position.y, elevatorDescendToProcessingTime).SetEase(Ease.InQuad);
        
        yield return new WaitForSeconds(elevatorDescendToProcessingTime - elevatorPrepareHighGearSFX.length);

        // Prepare high gear
        GameObject prepareSFX = ManagerSFX.Instance.PlaySFX(elevatorPrepareHighGearSFX, transform.position, 0.1f, false, _sfxMixer, parent: Elevator.transform).gameObject;
        prepareSFX.transform.position = Elevator.transform.position;

        yield return new WaitForSeconds(elevatorPrepareHighGearSFX.length - 0.6f);

        // Processing SFX
        _processingSFXObj = ManagerSFX.Instance.PlaySFX(elevatorProcessingSFX, transform.position, 0.5f, true, _sfxMixer, parent: Elevator.transform).gameObject;
        _processingSFXObj.transform.position = Elevator.transform.position;

        StartCoroutine(CrossfadeAudio(_processingSFXObj.GetComponent<AudioSource>(),  descendSFXObj.GetComponent<AudioSource>(), 1f));

        //
        moveElevator.Kill(complete: false);
        yield return new WaitForFixedUpdate();
        Elevator.transform.localPosition = new Vector3(Elevator.transform.localPosition.x, stoppingPoints[1].transform.localPosition.y, Elevator.transform.localPosition.z);
        //
        
        _elevatorShaftProcessing = StartCoroutine(ElevatorShaftProcessingUpdate());
        _elevatorShaftLightProcessing = StartCoroutine(ElevatorShaftLightProcessingUpdate());

        elevatorParticles.Play();
        ManagerPlayer.instance.PlayerCameraHelper.SetCameraShake(elevatorShakeNoise, 0.1f, 1f);
    }
    
    private IEnumerator CrossfadeAudio(AudioSource fadeIn, AudioSource fadeOut, float duration)
    {
        float time = 0f;
        
        float fadeInVolume = fadeIn.volume;
        fadeIn.volume = 0f;
        float currentFadeOutVolume = fadeOut.volume;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            fadeIn.volume = t * fadeInVolume;
            fadeOut.volume = currentFadeOutVolume * (1 - t);
            yield return null;
        }

        fadeIn.volume = fadeInVolume;
        fadeOut.volume = 0f;
        Destroy(fadeOut.gameObject);
    }

    private Coroutine _lockPlayerToElevator;

    private IEnumerator LockPlayerToElevatorUpdate()
    {
        while (true)
        {
            player.transform.position = new Vector3(player.transform.position.x, elevatorFloor.transform.position.y, player.transform.position.z);
            yield return null;
        }
    }
    
    private IEnumerator HandleGameplayFloorArrival()
    {
        yield return new WaitForFixedUpdate();
        
        // Processing SFX
        _processingSFXObj = ManagerSFX.Instance.PlaySFX(elevatorProcessingSFX, transform.position, 0.5f, true, _sfxMixer, parent: Elevator.transform).gameObject;
        _processingSFXObj.transform.position = Elevator.transform.position;
        
        // Init
        Elevator.transform.localPosition = new Vector3(Elevator.transform.localPosition.x, stoppingPoints[1].transform.localPosition.y, Elevator.transform.localPosition.z);
        _elevatorShaftProcessing = StartCoroutine(ElevatorShaftProcessingUpdate());
        _elevatorShaftLightProcessing = StartCoroutine(ElevatorShaftLightProcessingUpdate());

        elevatorParticles.Play();
        ManagerPlayer.instance.PlayerCameraHelper.SetCameraShake(elevatorShakeNoise, 0.1f, 1f);
        
        player.transform.position = Elevator.transform.position;
        _lockPlayerToElevator = StartCoroutine(LockPlayerToElevatorUpdate());
        
        // Wait
        yield return new WaitForSeconds(3f);
        
        // Clean
        elevatorParticles.Stop();
        StopCoroutine(_elevatorShaftProcessing);
        StopCoroutine(_elevatorShaftLightProcessing);
        ResetElevatorProcessingShaftPositions();
        ManagerPlayer.instance.PlayerCameraHelper.ClearCameraShake();
        
        // Resume
        float offsetTime = elevatorDescendToGameplaySFX.length - elevatorDescendToGameplayTime;
        GameObject descendToGameplaySFXObj = ManagerSFX.Instance.PlaySFX(elevatorDescendToGameplaySFX, transform.position, 0.1f, false, _sfxMixer, parent: Elevator.transform).gameObject;
        descendToGameplaySFXObj.transform.position = Elevator.transform.position;
        StartCoroutine(CrossfadeAudio(descendToGameplaySFXObj.GetComponent<AudioSource>(), _processingSFXObj.GetComponent<AudioSource>(), offsetTime));
        
        float offset = 0.3f;
        Elevator.transform.DOMoveY(stoppingPoints[2].position.y + offset, elevatorDescendToGameplayTime).SetEase(Ease.OutCirc);
        StartCoroutine(OnDepthCounter(floors[floorIndex].depth, elevatorDescendToGameplayTime));
        yield return new WaitForSeconds(elevatorDescendToGameplayTime + offsetTime);
        
        
        ManagerSFX.Instance.PlaySFX(elevatorStopSFX, Elevator.transform.position, 0.1f, false, _sfxMixer);
        
        float duration = 1f;
        Elevator.transform.DOMoveY(Elevator.transform.position.y - offset, duration).SetEase(Ease.OutBounce);

        float offsetTime1 = 0.7f;
        yield return new WaitForSeconds(duration - offsetTime1);
        OnFloorName();
        ManagerSFX.Instance.PlayRawSFX(floorTextRevealSFX, 0.2f, false, _sfxMixer);
        ManagerSFX.Instance.PlayAmbienceSFX(caveAmbienceSFX, 0.05f);
        yield return new WaitForSeconds(offsetTime1);
        
        ElevatorComplete();
    }
    
    private void ElevatorComplete()
    {
        if (_lockPlayerToElevator != null) StopCoroutine(_lockPlayerToElevator);
        
        ManagerAmbience.Instance.IsParticlesFollowing = true;
        IsInAction = false;
        hasProcessed = false;
        HasTakenElevator = false;

        StartCoroutine(OnFadeOutText());

        ManagerElevator.Instance.Arrived();
    }
    
    private Coroutine _elevatorShaftProcessing;
    private Vector3[] _shaftPiecesInitPos;
    private IEnumerator ElevatorShaftProcessingUpdate()
    {
        _shaftPiecesInitPos = new Vector3[shaftPieces.Length];
        for (int i = 0; i < shaftPieces.Length; i++)
        {
            _shaftPiecesInitPos[i] = shaftPieces[i].transform.position;
        }
        
        while (true)
        {
            foreach (GameObject shaft in shaftPieces)
            {
                shaft.transform.position += Vector3.up * elevatorShaftFallSpeed * Time.deltaTime;

                if (shaft.transform.localPosition.y > shaftPieceMaxYPos)
                {
                    shaft.transform.localPosition -= 3f * Vector3.up * shaftPieceHeight;

                    if (hasProcessed)
                    {
                        yield break;
                    }
                }
            }


            yield return null;
        }
    }

    private void ResetElevatorProcessingShaftPositions()
    {
        for (int i = 0; i < shaftPieces.Length; i++)
        {
            shaftPieces[i].transform.position = _shaftPiecesInitPos[i];
        }
    }
    
    private Coroutine _elevatorShaftLightProcessing;
    [SerializeField] private GameObject processingShaftLight;
    [SerializeField] private float processingShaftLightMaxYPos;
    [SerializeField] private float processingShaftLightMinYPos;
    [SerializeField] private float processingShaftLightFallSpeed;
    [SerializeField] private AudioSource processingShaftLightAudioSource;
    
    private IEnumerator ElevatorShaftLightProcessingUpdate()
    {
        Transform light = processingShaftLight.transform;
        processingShaftLightAudioSource.Play();
        
        while (true)
        {
            light.position += Vector3.up * processingShaftLightFallSpeed * Time.deltaTime;

            if (light.localPosition.y > processingShaftLightMaxYPos)
            {
                light.localPosition = new Vector3(light.localPosition.x, processingShaftLightMinYPos, light.localPosition.z);

                if (hasProcessed)
                {
                    yield break;
                }
            }

            yield return null;
        }
    }
}
