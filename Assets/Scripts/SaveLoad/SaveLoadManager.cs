using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Sirenix.OdinInspector;

public class SaveLoadManager : SingletonMonoBehavior<SaveLoadManager>
{
    public List<LevelData> levels = new List<LevelData>();
    private string savePath;
    private static int currentLevel = 0;
    private const string CURRENT_LEVEL_KEY = "CurrentLevel";
    private const string FINISHED_TUTORIAL_KEY = "FinishedTutorial";
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

    public bool IsFinishedTutorial
    {
        get => PlayerPrefs.GetInt(FINISHED_TUTORIAL_KEY, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(FINISHED_TUTORIAL_KEY, value ? 1 : 0);
            PlayerPrefs.Save();
        }
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

    [Button("Finish Tutorial")]
    private void SetFinishTutorial()
    {
        IsFinishedTutorial = true;
    }
    
    [Button("Clear All Data")]
    public void ClearAll()
    {
        PlayerPrefs.DeleteAll();
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
        levels.Clear();
        currentLevel = DEFAULT_LEVEL;
    }
}

[Serializable]
public class LevelData
{
    public LevelType levelType;
    public List<CharacterData> characterDatas = new List<CharacterData>();
    public DateTime SaveTime;
    public int currentRound;
}

[Serializable]
public class CharacterData
{
    public int iD;
    public CharacterType characterType;
    public Vector2Int points;
    public List<int> actionPoints = new List<int>();
    public int currentHp;
    public int currentMp;
    public IEffectInfo effectInfo;
    
    // Thay đổi: Sử dụng CharacterType thay vì string
    public bool isShadow = false;
    public int ownerID = -1;
    public CharacterType shadowType = CharacterType.CanSat; // Giá trị mặc định
    
    // Thêm trường cho Hoắc Liên Hương shield
    public Vector2Int? shieldCellPosition;
    
    // Thêm trường lưu MoveAmount 
    public int moveAmount;
}

[Serializable]
public class IEffectInfo
{
    [ShowInInspector] public List<EffectData> effects = new List<EffectData>();
}