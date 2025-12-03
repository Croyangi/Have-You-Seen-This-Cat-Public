using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Shapes;
using UnityEngine;

public class TutorialCatCollectionTerminal : CatCollectionTerminal
{
    protected override void GameStart()
    {
        // Do nothing
    }
    
    protected override void Start()
    {
        animationHelper.PlayExpression(animationHelper.expression);
        catQuotaText.text = "" + catQuota.ToString("D2");
        ManagerCat = ManagerCat.instance;
    }
    
    protected override IEnumerator OnMimicCollection(GameObject cat, CatStateMachine csm)
    {
        OnUnsuccessfulCollection();
        
        animationHelper.PlayExpression(CatCollectionTerminalAnimationHelper.Expressions.Error);
        
        isInTransit = false;
        isCopyCatError = true;

        ManagerSFX.Instance.PlaySFX(depositFailureSFX, transform.position, 0.05f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX);
        
        yield return new WaitForSeconds(1f);
        
        OnTurnOff();
        
        Destroy(cat);
        yield return new WaitForSeconds(1f);
        
        ResetAnimations();
        LeverCoroutine = StartCoroutine(OnLeverOff("mimicCollection"));
        animationHelper.ApplyQuota();
        isCopyCatError = false;
    }
    
    protected override void OnProcessCollection(GameObject catObj, CatStateMachine csm, Cat cat)
    {
        if (catObj.GetComponentInChildren<CatPhysicalModifierHelper>().isMimic)
        {
            StartCoroutine(OnMimicCollection(catObj, csm));
        }
        else
        {
            isInTransit = false;
            OnSuccessfulCollection();
            Destroy(catObj);
        }
    }
    
    private void OnUnsuccessfulCollection()
    {
        catStreak = 0;
        catCountText.text = CatCount.ToString("D2");
    }
    
    protected override void OnSuccessfulCollection()
    {
        catStreak++;
        
        int catAdd = 1 + catStreak / 3;

        float pitch = 1 + (catStreak * 0.05f);
        ManagerSFX.Instance.PlaySFX(depositSuccessSFX, transform.position, 0.05f, mixerGroup: ManagerAudioMixer.Instance.AMGSFX, pitchShift: pitch, isRandomPitch: false);
        CatCount += catAdd;
        
        catCountText.text = CatCount.ToString("D2");
        
        if (CatCount >= catQuota)
        {
            HasReachedQuota = true;
        }
    }
}
