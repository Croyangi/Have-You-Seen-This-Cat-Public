using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectedCat : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float sleepTime = 5f;
    [SerializeField] private bool isSleeping;
    [SerializeField] private float magnitudeThreshold;

    private void Awake()
    {
        StartCoroutine(StaticTimer());
    }

    private IEnumerator StaticTimer()
    {
        yield return new WaitForSeconds(sleepTime);
        isSleeping = true;
    }

    private void FixedUpdate()
    {
        if (!isSleeping) return;
        
        if (rb.linearVelocity.magnitude < magnitudeThreshold)
        {
            Destroy(rb);
            Destroy(this);
        }
    }
}
