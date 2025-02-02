#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;

public class LevelDesignManager : MonoBehaviour
{
    public LevelType levelType;
    public MapManager mapManager;
    public CharacterSpawnerConfig characterSpawnerConfig;
    public SerializableDictionary<CharacterType, string> gizmoNames;
    
    [TabGroup("Paths")] public string levelPrefabPath = "Assets/Prefabs/Gameplay/Levels";
    [TabGroup("Paths")] public string levelConfigPath = "Assets/ScriptableObject/Gameplay/Levels";
    
    private void OnValidate()
    {
        if (mapManager == null || characterSpawnerConfig == null) return;
        foreach (var cell in mapManager.Cells.Values)
        {
            cell.HideIcon();
        }
        foreach (var spawnPoint in characterSpawnerConfig.spawnPoints)
        {
            foreach (var cell in spawnPoint.Value.points.Select(point => mapManager.GetCell(point)))
            {
                cell.ShowIcon(gizmoNames[spawnPoint.Key]);
            }
        }
    }
    
    [Button("Save Level")]
    public void SaveLevel()
    {
        var levelConfig = ScriptableObject.CreateInstance<LevelConfig>();
        levelConfig.levelType = levelType;
        levelConfig.spawnerConfig = characterSpawnerConfig;
        
        var prefabName = $"Level_{levelType}.prefab"; 
        var prefabFullPath = System.IO.Path.Combine(levelPrefabPath, prefabName);
        
        GameObject prefabObject = PrefabUtility.SaveAsPrefabAsset(mapManager.gameObject, prefabFullPath);
        if (prefabObject == null)
        {
            //AlkawaDebug.LogError($"Failed to create prefab at path: {prefabFullPath}");
            return;
        }
        
        var prefabMapManager = prefabObject.GetComponent<MapManager>();
        if (prefabMapManager == null)
        {
            //AlkawaDebug.LogError("The saved prefab does not contain a MapManager component.");
            return;
        }
        levelConfig.mapPrefab = prefabMapManager;
        var assetName = $"Level_{levelType}.asset";
        var assetFullPath = System.IO.Path.Combine(levelConfigPath, assetName);
        AssetDatabase.CreateAsset(levelConfig, assetFullPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    
        //AlkawaDebug.Log($"Level saved successfully!\nPrefab path: {prefabFullPath}\nLevelConfig path: {assetFullPath}");
    }
    
    [Button("Load Level")]
    private void LoadLevel()
    {
        // Implement loading logic if needed
    }
}
#endif