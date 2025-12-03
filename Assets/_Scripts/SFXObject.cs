using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXObject : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioLowPassFilter audioLowPassFilter;
    public bool isOccluded;
    
    public float originalVolume;
}
