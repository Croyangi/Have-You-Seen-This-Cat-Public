using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode] [RequireComponent(typeof(Light))] 
public class SquareSpotlight : MonoBehaviour 
{ 
    [SerializeField] private int textureSize = 128; 
    [SerializeField] private float squareSize = 0.5f;

    private void OnValidate()
{
    Light spotLight = GetComponent<Light>();
    spotLight.type = LightType.Spot;
    spotLight.cookie = GenerateSquareCookie();
}

Texture2D GenerateSquareCookie()
{
    Texture2D cookie = new Texture2D(textureSize, textureSize);

    Color[] colors = new Color[textureSize * textureSize];
    int squareStart = (int)(textureSize * (0.5f - squareSize / 2));
    int squareEnd = (int)(textureSize * (0.5f + squareSize / 2));

    for (int y = 0; y < textureSize; y++)
    {
        for (int x = 0; x < textureSize; x++)
        {
            if (x >= squareStart && x <= squareEnd && y >= squareStart && y <= squareEnd)
                colors[y * textureSize + x] = Color.white;
            else
                colors[y * textureSize + x] = Color.black;
        }
    }

    cookie.SetPixels(colors);
    cookie.Apply();
    return cookie;
}
}
