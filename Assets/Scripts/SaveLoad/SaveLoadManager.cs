﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Sirenix.OdinInspector;

public class SaveLoadManager : SingletonMonoBehavior<SaveLoadManager>
{
    public List<LevelData> levels = new List<LevelData>();
    private string savePath;
    public static int currentLevel = 0;
    private const string CURRENT_LEVEL_KEY = "CurrentLevel";
    private const int DEFAULT_LEVEL = 0;

    protected override void Awake()
    {
        base.Awake();
        savePath = Application.persistentDataPath + "/levels.json";
        try
        {
            if (File.Exists(savePath))
            {
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.Auto, 
                };
                string json = File.ReadAllText(savePath);
                levels = JsonConvert.DeserializeObject<List<LevelData>>(json, settings);
            }
            else
            {
                levels = new List<LevelData>();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load levels: " + e.Message);
            levels = new List<LevelData>();
        }
    }

    private void Start()
    {
        LoadLevel();
    }

    public void OnSave(int index, LevelData levelData)
    {
        if (index < 0)
        {
            Debug.LogError("Index out of range");
            return;
        }
        if (index < levels.Count)
        {
            levels[index] = levelData;
        }
        else if (index == levels.Count)
        {
            levels.Add(levelData);
        }
        else
        {
            Debug.LogError("Index out of range");
            return;
        }
        try
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto, 
            };
            string json = JsonConvert.SerializeObject(levels, settings);
            File.WriteAllText(savePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save levels: " + e.Message);
        }
        
    }
    
    public LevelData OnLoad(int index)
    {
        if (index < 0 || index >= levels.Count)
        {
            Debug.LogError("Index out of range");
            return null;
        }
        return levels[index];
    }

    public static void LoadLevel()
    {
        currentLevel = PlayerPrefs.HasKey(CURRENT_LEVEL_KEY) ? PlayerPrefs.GetInt(CURRENT_LEVEL_KEY) : DEFAULT_LEVEL;
    }

    public void SetCurrentLevel(int level)
    {
        currentLevel = level;
        SaveLevel();
    }

    public static void SaveLevel()
    {
        PlayerPrefs.SetInt(CURRENT_LEVEL_KEY, currentLevel);
        PlayerPrefs.Save();
    }

    public void ClearAll()
    {
        PlayerPrefs.DeleteAll();
    }
}

[Serializable]
public class LevelData
{
    public LevelType levelType;
    public List<CharacterData> characterDatas = new List<CharacterData>();
    public DateTime SaveTime;
}

[Serializable]
public class CharacterData
{
    public int iD;
    public CharacterType characterType;
    public Vector2Int points;
    public int currentHp;
    public int currentMp;
    public IEffectInfo effectInfo;
}

[Serializable]
public class IEffectInfo
{
    [ShowInInspector] public List<EffectData> effects = new List<EffectData>();
}