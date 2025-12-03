using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DebugConsoleLog : MonoBehaviour
{
    private GUIStyle textAreaStyle;
    string myLog = "";

    //string filename = "";
    bool doShow = false;
    int kChars = 700;
    
    void OnEnable()
    {
        Application.logMessageReceived += Log;
        DebugToggle.togglePerformed += OnTogglePerformed;
        
        myLog = "";
        myLog += "<b>NEW LOG</b>";
    }

    void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }
    
    private void Start()
    {
        // Set up the custom style for TextArea
        textAreaStyle = new GUIStyle();
        //textAreaStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.7f));  // Darker background
        //textAreaStyle.normal.textColor = Color.white;  // Set text color to white
        textAreaStyle.fontSize = 10;  // Set font size
        textAreaStyle.richText = true;
        //textAreaStyle.wordWrap = true;
        //textAreaStyle.clipping = TextClipping.Overflow;
    }
    
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        texture.SetPixels(pix);
        texture.Apply();
        return texture;
    }

    private void OnTogglePerformed()
    {
        doShow = !doShow;
    }

    private void Log(string logString, string stackTrace, LogType type)
    {
        // for onscreen...
        myLog = myLog + "\n" + logString;
        if (myLog.Length > kChars)
        {
            myLog = myLog.Substring(myLog.Length - kChars);
        }
    }
    
    private void OnGUI()
    {
        if (!doShow) { return; }

        // Calculate a Rect that fits a third of the screen and positions it in the top left
        int width = (int) (Screen.width / 3.5f);
        int height = (int) (Screen.height / 1.5f);
        float xPos = 10;  // 10px padding from the left edge
        float yPos = 10;  // 10px padding from the top edge
        
        // Create the TextArea with the calculated dimensions and position
        GUI.TextArea(new Rect(xPos, yPos, width, height), myLog);
    }
}