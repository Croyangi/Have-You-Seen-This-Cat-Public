using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VersionText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;

    private void Awake()
    {
        textMesh.text = "v" + Application.version;
    }
}
