using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterManager : SingletonMonoBehavior<CharacterManager>
{
    [SerializeField] private SerializableDictionary<CharacterType, Character> allCharacter = new();

    // private
    private List<Character> Players { get; set; } = new();
    private List<Character> Enemies { get; set; } = new();


    public MapManager MapManager { get; set; }
    private CharacterSpawnerConfig _spawnerConfig;
    
    // public 
    public Character SelectedCharacter { get; set; }
    public Character MainCharacter { get; set; }
    public Character FocusedCharacter { get; set; }
    public List<Character> Characters { get; set; } = new();
    public HashSet<Character> CharactersInRange { get; set; } = new();
    public GameplayManager GPManager => GameplayManager.Instance;
    
    public void Initialize(CharacterSpawnerConfig config, MapManager mapManager)
    {
        _spawnerConfig = config;
        MapManager = mapManager;
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
                character.Initialize(this, MapManager.GetCell(point));
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

                if (GPManager.IsTutorialLevel)
                {
                    character.HideHpBar();
                }
            }
        }

        SortCharacterBySpeed();
        if (!GPManager.IsTutorialLevel) SetMainCharacter();
    }

    private void SortCharacterBySpeed()
    {
        Characters = Characters.OrderByDescending(c => c.characterInfo.Speed).ToList();
    }

    public void SetMainCharacter()
    {
        SetMainCharacter(Characters[GPManager.CharacterIndex]);
    }

    public void SetMainCharacter(Character character)
    {
        MainCharacter = character;
        MainCharacter.SetMainCharacter();
        SetSelectedCharacter(character);
        HUD.Instance.SetupCharacterFocus(Characters, GPManager.CharacterIndex);
        if (Characters.Count > 0 && character == Characters[0])
        {
            GPManager.HandleNewRound();
        }
    }
    
    public void SetSelectedCharacter(Character character)
    {
        Debug.Log($"Gameplay: SetSelectedCharacter: {character.characterConfig.characterName}");
        SelectedCharacter?.OnUnSelected();
        SelectedCharacter = character;
        HideMoveRange();
        HideSkillRange();
        if (!GPManager.IsTutorialLevel) character.OnSelected();
        HUD.Instance.SetCharacterFocus(GetSelectedCharacterParams());
    }

    public void HandleCastSkill(Character character)
    {
        Debug.Log("NT - Handle Cast Skill");
        HideMoveRange();
        HideSkillRange();
        FocusedCharacter = character;
        SelectedCharacter.characterInfo.OnCastSkill(GPManager.SkillInfo, OnCastSkillFinished);
    }

    private void OnCastSkillFinished()
    {
        Debug.Log("NT - On Cast Skill Finished");
        TryAttackEnemies(FocusedCharacter);
    }
    
    private bool TryAttackEnemies(Character focusedCharacter)
    {
        var skillInfo = GPManager.SkillInfo;
        // FocusedCharacter.characterInfo.OnDamageTaken(5, SetDamageTakenFinished);
        // check crit
        int damage = SelectedCharacter.characterInfo.BaseDamage;
        var hitChange = SelectedCharacter.Roll.GetHitChange();
        var isCritical = SelectedCharacter.Roll.IsCritical(hitChange);
        if (!isCritical)
        {
            var dodge = focusedCharacter.characterInfo.Dodge;
            if (dodge > hitChange)
            {
                Debug.Log($"[Gameplay] Né skill, dodge = {dodge} - hitChange = {hitChange}");
                focusedCharacter.characterInfo.OnDamageTaken(0, SetDamageTakenFinished);
            }
            else
            {
                if (skillInfo.hasApplyDamage)
                {
                    damage += SelectedCharacter.Roll.RollDice(skillInfo.damageConfig);
                }
                // 
                if (skillInfo.effectIndex != EffectIndex.None)
                {
                    // var debuffResistance = RollManager.Instance.GetDebuffResistance(focusedCharacter.Info.CharacterAttributes);
                    // if (debuffResistance < GameplayManager.EffectConstant
                    //         .effectData[_skillData.effectConfig.effectIndex].debuffResistance)
                    // {
                    //     focusedCharacter.Info.ApplyDeBuff(_skillData.effectConfig, SelectedCharacter);
                    //     Debug.Log($"[Gameplay] - Damage: {damage}");
                    // }
                }
                focusedCharacter.characterInfo.OnDamageTaken(damage, SetDamageTakenFinished);
            }
        }
        
        else
        {
            damage *= 2;
            focusedCharacter.characterInfo.OnDamageTaken(damage, SetDamageTakenFinished);
            Debug.Log($"Gameplay: Get Damage Taken {damage}");
        }
        
        return false;
    }

    private void SetDamageTakenFinished()
    {
        if (FocusedCharacter is PlayerCharacter && MainCharacter is AICharacter)
        {
            ReactMenu.Instance.Open();
        }
    }

    public Character GetCharacterByType(CharacterType characterType)
    {
        return Characters.FirstOrDefault(character => character.characterType == characterType);
    }
    
    #region Sub

    private CharacterParams GetSelectedCharacterParams()
    {
        return new CharacterParams
        {
            Character = SelectedCharacter,
            Skills = SelectedCharacter.GetSkillInfos(GetSkillType())
        };
    }
    
    public void ShowAllHPBar()
    {
        foreach (var character in Characters)
        {
            character.ShowHpBar();
        }
    }

    public bool IsMainCharacterSelected => SelectedCharacter == MainCharacter;

    public bool CanShowEndTurn => IsMainCharacterSelected && MainCharacter is PlayerCharacter;
    #endregion

    #region Cell Interact

    public void ShowMoveRange()
    {
        var range = SelectedCharacter.characterInfo.GetMoveRange();
        Debug.Log($"Gameplay: [{SelectedCharacter.characterConfig.characterName}] Show Move Range: {range}");
        MapManager.ShowMoveRange(SelectedCharacter.characterInfo.Cell, range);
    }

    public void HideMoveRange()
    {
        Debug.Log($"Gameplay: [{SelectedCharacter.characterConfig.characterName}] Hide Move Range");
        MapManager.HideMoveRange();
    }

    public void HideSkillRange()
    {
        Debug.Log($"Gameplay: [{SelectedCharacter.characterConfig.characterName}] Hide Skill Range");
        MapManager.HideSkillRange();
        CharactersInRange.Clear();
    }

    public bool TryMoveToCell(Cell cell)
    {
        if (SelectedCharacter is PlayerCharacter && SelectedCharacter != null && MapManager.CanMove(cell))
        {
            var cellPath = MapManager.FindPath(SelectedCharacter.characterInfo.Cell, cell);
            MapManager.HideMoveRange();
            SelectedCharacter.MoveCharacter(cellPath);
            return true;
        }
        return false;
    }
    
    public List<Character> GetEnemiesInRange(Character character, int range)
    {
        var characters = MapManager.GetCharacterInRange(character.characterInfo.Cell, range);
        return characters.Where(c => c.Type != character.Type).ToList();
    }
    
    #endregion

    #region Facing

    public SkillType GetSkillType()
    {
        if (SelectedCharacter == MainCharacter) return SkillType.InternalSkill;
        return SelectedCharacter.Type == MainCharacter.Type ? SkillType.MovementSkill : SkillType.CombatSkill;
    }
    
    public SkillType GetSkillType(Character character)
    {
        if (character == MainCharacter) return SkillType.InternalSkill;
        return character.Type == MainCharacter.Type ? SkillType.MovementSkill : SkillType.CombatSkill;
    }

    public SkillInfo GetSkillInfo(int index)
    {
        return SelectedCharacter.characterInfo.GetSkillInfo(index, GetSkillType());
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

    #endregion
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