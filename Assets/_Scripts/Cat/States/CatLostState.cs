using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatLostState : State<CatStateMachine>
{
    [Header("References")] 
    [SerializeField] private Cat cat;
    [SerializeField] private CatMovementHelper movementHelper;
    [SerializeField] private CatStateHelper stateHelper;
    [SerializeField] private CatAnimationHelper animationHelper;

    [Header("Settings")] 
    [SerializeField] private float movementSpeed;

    [SerializeField] private float meowTimer;
    [SerializeField] private float meowTimerSet;
    
    [SerializeField] private AudioClip[] meowSounds;

    public override void EnterState()
    {
        movementHelper.SetMovementSpeed(movementSpeed);
        animationHelper.ToggleIdleAnimations(true);
        meowTimer = meowTimerSet;
        cat.SetIsFound(false);
        cat.GetComponent<InteractableObject>().IsInspectable = false;
    }

    public override void FixedUpdateTick()
    {
        if (meowTimer <= 0)
        {
            PlayMeowSFX();
            meowTimer = meowTimerSet;
        }
        else
        {
            meowTimer = Mathf.Max(0, meowTimer -= Time.fixedDeltaTime);
        }
    }

    public override void ExitState()
    {
        if (ManagerSFX.Instance != null) PlayMeowSFX();
        animationHelper.ToggleIdleAnimations(false);
    }

    private void PlayMeowSFX()
    {
        SFXObject temp = ManagerSFX.Instance.PlaySFX(meowSounds[Random.Range(0, meowSounds.Length)], transform.position, 0.03f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: 0.1f);
        ManagerSFX.Instance.ApplyLowPassFilter(temp);
    }
}
