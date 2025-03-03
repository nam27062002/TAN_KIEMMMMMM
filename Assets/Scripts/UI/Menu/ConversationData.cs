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
        public bool hasSpawnCharacter;
        [ShowIf("hasSpawnCharacter")] public List<SpawnCharacter> spawnCharacters = new();
        public bool shake;
    }
        
    public List<Data> conversation = new List<Data>();
    
    [Serializable]
    public class SpawnCharacter
    {
        public Character character;
        public Vector2 position;
        public bool canMove;
        public FacingType facingType = FacingType.Right;
        [ShowIf("canMove")] public Vector2 targetPosition;
    }
}