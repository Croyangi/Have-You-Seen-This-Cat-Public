using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pathfinding;
using Shapes;
using Unity.Collections;
using UnityEngine.Rendering;

public class Test : MonoBehaviour
{
	[SerializeField] private AudioClip clip;

	private void Start()
	{
		//GameObject obj = ManagerSFX.Instance.PlaySFX(clip, transform.position, isLooping: true, parent: transform);
		//ManagerSFX.Instance.ApplyLowPassFilter(obj.GetComponent<SFXObject>());
	}
}
