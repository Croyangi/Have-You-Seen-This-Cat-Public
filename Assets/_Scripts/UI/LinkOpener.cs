using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkOpener : MonoBehaviour
{
    [SerializeField] private string url = "https://example.com";

    // Call this method to open the link
    public void OpenURL()
    {
        Application.OpenURL(url);
    }
}
