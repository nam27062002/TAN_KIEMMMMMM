using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "SO/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public string levelName;
    public LevelType levelType;
    public CharacterSpawnerConfig spawnerConfig;
    public MapManager mapPrefab;
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