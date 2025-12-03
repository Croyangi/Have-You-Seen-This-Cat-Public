using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdvisoryHelper : MonoBehaviour
{
    [SerializeField] private Image logo;
    [SerializeField] private List<TextMeshProUGUI> contentWarnings;

    private IEnumerator Start()
    {
        logo.gameObject.SetActive(true);
        logo.color = new Color(1, 1, 1, 0);
        foreach (var t in contentWarnings) t.alpha = 0f;
        foreach (var t in contentWarnings) t.gameObject.SetActive(false);
        
        Sequence logoSeq = DOTween.Sequence();
        logoSeq.AppendInterval(1f);
        logoSeq.Append(logo.DOFade(1f, 1f));
        logoSeq.AppendInterval(1f);
        logoSeq.Append(logo.DOFade(0f, 1f));
        
        yield return logoSeq.WaitForCompletion();
        
        foreach (var t in contentWarnings) t.gameObject.SetActive(true);
        Sequence textSeq = DOTween.Sequence();
        foreach (var t in contentWarnings) textSeq.Join(t.DOFade(1f, 2f));
        textSeq.AppendInterval(1f);
        foreach (var t in contentWarnings) textSeq.Join(t.DOFade(0f, 2f));
        
        yield return textSeq.WaitForCompletion();
        
        SceneLoader.Load(SceneID.MainMenu);
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            SceneLoader.Load(SceneID.MainMenu);
        }
    }
}