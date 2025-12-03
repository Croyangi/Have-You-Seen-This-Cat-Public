using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class SettingsData
{
    public float masterVolume = 1;
    public float musicVolume = 1;
    public float sfxVolume = 1;
}

[Serializable]
public class ExpeditionData
{
    public bool isOngoing = false;
    public int floor = 0;
    public int time = 0;
    public int catsCollected = 0;
}

[Serializable]
public class PlayerData
{
    public string name;
    public int deaths;
}

[Serializable]
public class GameData
{
    public bool hasCompletedTutorial;
    public bool hasTakenElevator;
    public bool hasDied;
    
    public SettingsData settings;
    public ExpeditionData expedition;
    public PlayerData player;
    

    public GameData()
    {
        settings = new SettingsData();
        expedition = new ExpeditionData();
    }
}