using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterManager : SingletonMonoBehavior<CharacterManager>
{
    [SerializeField] private SerializableDictionary<CharacterType, Character> allCharacter = new();
    
    private List<Character> Players { get; set; } = new();
    private List<Character> Enemies { get; set; } = new();
    private List<Character> Characters { get; set; } = new();
    
    private MapManager _mapManager;
    private CharacterSpawnerConfig _spawnerConfig;
    private int _characterIndex = 0;

    private Character SelectedCharacter { get; set; }
    private Character MainCharacter { get; set; }
    
    public void Initialize(CharacterSpawnerConfig config, MapManager mapManager)
    {
        _spawnerConfig = config;
        _mapManager = mapManager;
        SpawnCharacter();
    }

    private void SpawnCharacter()
    {
        Characters.Clear();
        Enemies.Clear();
        Players.Clear();
        
        foreach (var spawnPoint in _spawnerConfig.spawnPoints)
        {
            foreach (var point in spawnPoint.Value.points)
            {
                var go = Instantiate(allCharacter[spawnPoint.Key], transform);
                var character = go.GetComponent<Character>();
                character.Initialize(this, _mapManager.GetCell(point));
                Characters.Add(character);
                switch (character)
                {
                    case AICharacter aiCharacter:
                        Enemies.Add(aiCharacter);
                        break;
                    case PlayerCharacter playerCharacter:
                        Players.Add(playerCharacter);
                        break;
                }
            }
        }
        
        SortCharacterBySpeed();
        SetMainCharacter();
    }
    
    private void SortCharacterBySpeed()
    {
        Characters = Characters.OrderByDescending(c => c.characterInfo.Speed).ToList();
    }
    
    private void SetMainCharacter()
    {
        SetMainCharacter(Characters[_characterIndex]);
    }
    
    private void SetMainCharacter(Character character)
    {
        MainCharacter = character;
        SetSelectedCharacter(character); 
        HUD.Instance.SetupCharacterFocus(Characters, _characterIndex);
        HUD.Instance.SetCharacterFocus(GetSelectedCharacterParams());
        
        if (Characters.Count > 0 && character == Characters[0])
        {
            GameplayManager.Instance.HandleNewRound();
        }
    }

    private void SetSelectedCharacter(Character character)
    {
        SelectedCharacter = character;
    }

    private CharacterParams GetSelectedCharacterParams()
    {
        return new CharacterParams
        {
            Character = SelectedCharacter,
            Skills = SelectedCharacter.GetSkillInfos(GetSkillType())
        };
    }
    
    private SkillType GetSkillType()
    {
        if (SelectedCharacter == MainCharacter) return SkillType.InternalSkill;
        return SelectedCharacter.Type == MainCharacter.Type ? SkillType.MovementSkill : SkillType.CombatSkill;
    }
    
    public FacingType GetFacingType(Character character)
    {
        if (character == null)
        {
            Debug.LogWarning("GetFacingType: Character is null.");
            return FacingType.Right;
        }
        var characterPosition = character.transform.position;
        var nearestOpponent = Utils.FindNearestCharacter(character, character is AICharacter ? Players : Enemies);
        if (nearestOpponent == null)
        {
            Debug.LogWarning("GetFacingType: No opponents found.");
            return FacingType.Right;
        }
        var opponentPosition = nearestOpponent.transform.position;
        return characterPosition.x > opponentPosition.x ? FacingType.Left : FacingType.Right;
    }
}

public enum FacingType
{
    Left,
    Right,
}

[Serializable]
public enum Type
{
    None,
    Player,
    AI,
}