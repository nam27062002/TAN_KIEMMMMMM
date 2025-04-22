using System;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject(ItemTypeNameHandling = TypeNameHandling.Auto)]
[Serializable]
public class EffectData
{
    public EffectType effectType;
    public int duration;
    [NonSerialized] public Character Actor;
    public int characterId;
    
    // Thêm phương thức này để xử lý trước khi lưu
    public virtual void OnBeforeSave()
    {
        // Lưu characterId từ Actor
        if (Actor != null)
        {
            characterId = Actor.CharacterId;
        }
    }
    
    // Thêm phương thức này để xử lý khi load
    public virtual void OnAfterLoad(MapManager mapManager)
    {
        // Khôi phục Actor từ characterId
        if (characterId != 0)
        {
            Actor = mapManager.GetCharacterById(characterId);
            if (Actor == null)
            {
                UnityEngine.Debug.LogWarning($"Character with ID {characterId} not found for effect {effectType}");
            }
        }
    }
}