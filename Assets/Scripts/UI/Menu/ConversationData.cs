using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ConversationData", menuName = "SO/Menu/Conversation Data")]
public class ConversationData : ScriptableObject
{
    [Serializable]
    public class Data
    {
        public bool useAvt = true;
        [ShowIf(nameof(useAvt))] public Sprite avatar;
        public string text;
    }
        
    public List<Data> conversation = new List<Data>();
}