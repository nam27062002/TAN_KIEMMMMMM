using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameplayManager : SingletonMonoBehavior<GameplayManager>
{
    [SerializeField] private LevelConfig levelConfig;  
    
    // private
    private MapManager MapManager { get; set; }
    private int CurrentRound { get; set; } = 0;
    private bool InPlayerTurn => characterManager.MainCharacter is PlayerCharacter;
    public SkillInfo SkillInfo { get; set; }
    
    // public
    public CharacterManager characterManager;
    public int CharacterIndex { get; set; }

    public bool IsTutorialLevel => levelConfig.levelType == LevelType.Tutorial;
    // Event
    public event EventHandler OnNewRound;
    
    protected override void Awake()
    {
        base.Awake();
        DOTween.Init(false, false, LogBehaviour.ErrorsOnly);
        DOTween.SetTweensCapacity(500, 125);
    }
    
    private void Start()
    {
        StartNewGame();
    }

    private void StartNewGame()
    {
        HUD.Instance.HideHUD();
        CurrentRound = 0;
        HUD.Instance.SetLevelName(levelConfig.levelName);
        
        if (!IsTutorialLevel) 
            LoadMapGame();
    }
    
    private void LoadMapGame()
    {
        var go = Instantiate(levelConfig.mapPrefab, transform);
        MapManager = go.GetComponent<MapManager>();
        MapManager.Initialize();
    }

    public void LoadCharacter()
    {
        characterManager.Initialize(levelConfig.spawnerConfig, MapManager);
        HUD.Instance.ShowHUD();
    }

    public void HandleNewRound()
    {
        CurrentRound++;
        OnNewRound?.Invoke(this, EventArgs.Empty);
        Debug.Log($"NT - Gameplay: round {CurrentRound}");
    }
    
    public void OnCellClicked(Cell cell)
    {
        switch (cell.CellType)
        {
            case CellType.Character:
                OnCharacterClicked(cell);
                break;
            case CellType.Walkable:
                OnWaypointClicked( cell);
                break;
        }
    }
    
    private void OnCharacterClicked(Cell cell)
    {
        if (characterManager.CharactersInRange.Contains(cell.Character))
        {
            HandleCastSkill(cell.Character);
        }
        else
        {
            if (InPlayerTurn)
            {
                if (cell.Character.Type == Type.AI)
                {
                    characterManager.SetSelectedCharacter(cell.Character);
                }
                else
                {
                    if (cell.Character == characterManager.SelectedCharacter)
                    {
                        UnSelectSkill();
                        if (characterManager.IsMainCharacterSelected) characterManager.ShowMoveRange();
                    }
                    else
                    {
                        characterManager.SetSelectedCharacter(cell.Character);
                    }
                }
            }   
        }
    }

    private void HandleCastSkill(Character character)
    {
        characterManager.HandleCastSkill(character);
    }
    
    private void OnWaypointClicked(Cell cell)
    {
        if (InPlayerTurn)
        {
            characterManager.TryMoveToCell(cell);
        }
    }

    public void HandleEndTurn()
    {
        Debug.Log("NT - Gameplay: end turn");
        CharacterIndex++;
        if (CharacterIndex >= characterManager.Characters.Count)
        {
            CharacterIndex = 0;
        }
        characterManager.MainCharacter.characterInfo.ResetBuffAfter();
        ResetAllChange();
        characterManager.SetMainCharacter();
    }

    public void HandleSelectSkill(int skillIndex)
    {
        Debug.Log($"NT - Gameplay: select skill {skillIndex}");
        characterManager.HideMoveRange();
        UnSelectSkill();
        if (SkillInfo != GetSkillInfo(skillIndex))
        {
            characterManager.HideSkillRange();
            SkillInfo = GetSkillInfo(skillIndex);
            if (SkillInfo.isDirectionalSkill)
            {
                HandleDirectionalSkill();
            }
            else
            {
                HandleNonDirectionalSkill();
            }
        }
    }

    private void HandleDirectionalSkill()
    {
        // show skill range
        if (SkillInfo.range > 0)
        {
            MapManager.ShowSkillRange(characterManager.SelectedCharacter.characterInfo.Cell, SkillInfo.range);
            Debug.Log($"Gameplay: Show skill range: {SkillInfo.range}");
        }
        
        // get character can be interact
        // if (_skillData.damageTargetType.HasFlag(DamageTargetType.Self))
        // {
        //     CharactersInRange.Add(SelectedCharacter);
        // }

        foreach (var cell in MapManager.SkillRange.Where(cell => cell.CellType == CellType.Character))
        {
            if (cell.Character.Type == characterManager.SelectedCharacter.Type 
                // && _skillData.damageTargetType.HasFlag(DamageTargetType.Team)
                )
            {
                characterManager.CharactersInRange.Add(cell.Character);
            }

            if (cell.Character.Type != characterManager.SelectedCharacter.Type 
                // && _skillData.damageTargetType.HasFlag(DamageTargetType.Enemies)
                )
            {
                characterManager.CharactersInRange.Add(cell.Character);
            }
        }
    }

    private void HandleNonDirectionalSkill()
    {
        
    }
    
    private void UnSelectSkill()
    {
        if (SkillInfo != null)
        {
            
        }
        SkillInfo = null;
        characterManager.HideSkillRange();
    }

    private SkillInfo GetSkillInfo(int index)
    {
        return characterManager.GetSkillInfo(index);
    }

    private void ResetAllChange()
    {
        
    }
}
