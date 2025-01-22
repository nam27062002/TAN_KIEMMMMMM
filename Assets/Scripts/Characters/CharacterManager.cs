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


    private MapManager _mapManager;
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
        character.OnSelected();
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
        FocusedCharacter.characterInfo.OnDamageTaken(5, SetDamageTakenFinished);
        // check crit
        // int damage = SelectedCharacter.Info.BaseDamage;
        // var isCritical = RollManager.Instance.IsCritical(SelectedCharacter.Info.CharacterAttributes.rollValue);
        // if (!isCritical)
        // {
        //     var hitChange = RollManager.Instance.GetHitChange(SelectedCharacter.Info.CharacterAttributes);
        //     var dodge = focusedCharacter.Info.Dodge;
        //     if (dodge > hitChange)
        //     {
        //         Debug.Log($"[Gameplay] Né skill, dodge = {dodge} - hitChange = {hitChange}");
        //     }
        //     else
        //     {
        //         if (_skillData.hasApplyDamage)
        //         {
        //             damage += RollManager.RollDice(_skillData.damageConfig.rollData);
        //         }
        //         // 
        //         if (_skillData.effectConfig.effectIndex != EffectIndex.None)
        //         {
        //             var debuffResistance = RollManager.Instance.GetDebuffResistance(focusedCharacter.Info.CharacterAttributes);
        //             if (debuffResistance < GameplayManager.EffectConstant
        //                     .effectData[_skillData.effectConfig.effectIndex].debuffResistance)
        //             {
        //                 focusedCharacter.Info.ApplyDeBuff(_skillData.effectConfig, SelectedCharacter);
        //                 Debug.Log($"[Gameplay] - Damage: {damage}");
        //             }
        //         }
        //     }
        // }
        //
        // else
        // {
        //     damage *= 2;
        //     focusedCharacter.Info.OnDamageTaken(damage);
        //     Debug.Log($"Gameplay: Get Damage Taken {damage}");
        // }
        //
        return false;
    }

    private void SetDamageTakenFinished()
    {
        Debug.Log("NT - SetDamageTakenFinished");
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

    public bool IsMainCharacterSelected => SelectedCharacter == MainCharacter;

    public bool CanShowEndTurn => IsMainCharacterSelected && MainCharacter is PlayerCharacter;
    #endregion

    #region Cell Interact

    public void ShowMoveRange()
    {
        var range = SelectedCharacter.characterInfo.GetMoveRange();
        Debug.Log($"Gameplay: [{SelectedCharacter.characterConfig.characterName}] Show Move Range: {range}");
        _mapManager.ShowMoveRange(SelectedCharacter.characterInfo.Cell, range);
    }

    public void HideMoveRange()
    {
        Debug.Log($"Gameplay: [{SelectedCharacter.characterConfig.characterName}] Hide Move Range");
        _mapManager.HideMoveRange();
    }

    public void HideSkillRange()
    {
        Debug.Log($"Gameplay: [{SelectedCharacter.characterConfig.characterName}] Hide Skill Range");
        _mapManager.HideSkillRange();
        CharactersInRange.Clear();
    }

    public bool TryMoveToCell(Cell cell)
    {
        if (SelectedCharacter is PlayerCharacter && SelectedCharacter != null && _mapManager.CanMove(cell))
        {
            var cellPath = _mapManager.FindPath(SelectedCharacter.characterInfo.Cell, cell);
            _mapManager.HideMoveRange();
            SelectedCharacter.MoveCharacter(cellPath);
            return true;
        }
        return false;
    }
    #endregion

    #region Facing

    public SkillType GetSkillType()
    {
        if (SelectedCharacter == MainCharacter) return SkillType.InternalSkill;
        return SelectedCharacter.Type == MainCharacter.Type ? SkillType.MovementSkill : SkillType.CombatSkill;
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