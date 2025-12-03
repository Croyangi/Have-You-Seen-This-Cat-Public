using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperSpeedRamp : MonoBehaviour
{
    [SerializeField] private float speed;
    
    private void OnTriggerStay(Collider other)
    {
        Vector3 vel = other.gameObject.GetComponent<Rigidbody>().linearVelocity;
        vel.x *= speed;
        vel.z *= speed;
        other.gameObject.GetComponent<Rigidbody>().linearVelocity = vel;
    }
}
