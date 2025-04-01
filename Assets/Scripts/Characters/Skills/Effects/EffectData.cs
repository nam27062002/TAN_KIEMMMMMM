using System;
using Newtonsoft.Json;

[JsonObject(ItemTypeNameHandling = TypeNameHandling.Auto)]
[Serializable]
public class EffectData
{
    public EffectType effectType;
    public int duration;
    [NonSerialized] public Character Actor;
    public int characterId;
    
    // Thêm phương thức này để xử lý khi load
    public virtual void OnAfterLoad(MapManager mapManager)
    {
        // Xử lý chung cho tất cả hiệu ứng
    }
}