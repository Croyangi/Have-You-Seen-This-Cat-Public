using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugFPS : MonoBehaviour, IDebugCommandSource
{
    [Header("References")]
    [SerializeField] private GameObject fpsUI;
    [SerializeField] private float minFPS = Mathf.Infinity;
    [SerializeField] private float maxFPS = Mathf.NegativeInfinity;
    [SerializeField] private float currentFPS;
    [SerializeField] private TextMeshProUGUI tmpMinFPS;
    [SerializeField] private TextMeshProUGUI tmpMaxFPS;
    [SerializeField] private TextMeshProUGUI tmpCurrentFPS;
    public bool isShowing;
    
    private void OnEnable()
    {
        currentFPS = (1f / Time.deltaTime);
        minFPS = Mathf.Infinity;
        maxFPS = Mathf.NegativeInfinity;
        
        StopAllCoroutines();
        StartCoroutine(WarmUpFPS());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public IEnumerable<DebugCommand> GetCommands()
    {
        yield return new DebugCommand(
            "fps_toggle",
            "Toggles showing FPS.",
            "fps_toggle",
            args =>
            {
                OnTogglePerformed();
            }
            
        );
        
        yield return new DebugCommand(
            "fps_refresh",
            "Refreshes FPS.",
            "fps_refresh",
            args =>
            {
                RefreshFPS();
            }
            
        );
    }

    private void OnTogglePerformed()
    {
        isShowing = !isShowing;
        fpsUI.SetActive(isShowing);
        
        StopAllCoroutines();
        StartCoroutine(WarmUpFPS());
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus && isShowing)
        {
            StopAllCoroutines();
            StartCoroutine(DisplayFPS());
        } else
        {
            StopAllCoroutines();
        }
    }

    private void RefreshFPS()
    {
        minFPS = Mathf.Infinity;
        maxFPS = Mathf.NegativeInfinity;
    }

    private void Update()
    {
        currentFPS = (1f / Time.deltaTime);
    }

    private IEnumerator WarmUpFPS()
    {
        yield return new WaitForSeconds(3f);
        StartCoroutine(DisplayFPS());
    }

    private IEnumerator DisplayFPS()
    {
        while (isShowing)
        {
            tmpCurrentFPS.text = "FPS: " + currentFPS.ToString("F0");

            if (currentFPS < minFPS)
            {
                minFPS = currentFPS;
                tmpMinFPS.text = "Min: " + minFPS.ToString("F0");
            }

            if (currentFPS > maxFPS)
            {
                maxFPS = currentFPS;
                tmpMaxFPS.text = "Max: " + maxFPS.ToString("F0");
            }
            
            yield return new WaitForSeconds(0.2f);
        }
    }
}