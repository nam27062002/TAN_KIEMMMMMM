using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "SO/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public string levelName;
    public float cameraSize;
    public LevelType levelType;
    public CharacterSpawnerConfig spawnerConfig;
    public CharacterSpawnerConfig specialSpawnerConfig;
    public MapManager mapPrefab;
    public List<ConversationData> startConversations = new List<ConversationData>();
    public List<ConversationData> winConversations = new List<ConversationData>();
    public LevelConfig nextLevel;
}

[Serializable]
public class CharacterSpawnerConfig
{
    public SerializableDictionary<CharacterType, Points> spawnPoints = new();
}

[Serializable]
public class Points
{
    public List<Vector2Int> points = new();
}