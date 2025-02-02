using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameplayManager : SingletonMonoBehavior<GameplayManager>
{
    [SerializeField] private LevelConfig levelConfig;  
    [SerializeField] private GameObject tutorialPrefab;
    // private
    private MapManager MapManager { get; set; }
    public int CurrentRound { get; set; } = 0;
    private bool InPlayerTurn => characterManager.MainCharacter is PlayerCharacter;
    public SkillInfo SkillInfo { get; set; }
    
    // public
    public CharacterManager characterManager;
    [ShowInInspector] public int CharacterIndex { get; set; }

    public bool IsTutorialLevel;
    public CharacterParams ShowInfoCharacterParams { get; set; }
    //public bool IsTutorialLevel => false;
    // Event
    public event EventHandler OnNewRound;
    public event EventHandler OnLoadCharacterFinished;
    
    protected override void Awake()
    {
        base.Awake();
        UIManager.Instance.OpenMenu(MenuType.InGame);
        // IsTutorialLevel = levelConfig.levelType == LevelType.Tutorial;
        // if (IsTutorialLevel)
        // {
        //     tutorialPrefab.SetActive(true);
        // }
    }
    
    private void Start()
    {
        //StartNewGame();
    }

    private void StartNewGame()
    {
        // HUD.Instance.HideHUD();
        // CurrentRound = 0;
        // HUD.Instance.SetLevelName(levelConfig.levelName);
        //
        // if (!IsTutorialLevel) 
        //     LoadMapGame();
    }
    
    public void LoadMapGame()
    {
        var go = Instantiate(levelConfig.mapPrefab, transform);
        MapManager = go.GetComponent<MapManager>();
        MapManager.Initialize();
    }

    public void LoadCharacter()
    {
        characterManager.Initialize(levelConfig.spawnerConfig, MapManager);
        // if (!IsTutorialLevel) HUD.Instance.ShowHUD();
        OnLoadCharacterFinished?.Invoke(this, EventArgs.Empty);
    }

    public void HandleNewRound()
    {
        CurrentRound++;
        OnNewRound?.Invoke(this, EventArgs.Empty);
        // HUD.Instance.SetRound();
        //AlkawaDebug.Log($"NT - Gameplay: round {CurrentRound}");
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

    public void HandleCastSkill(Character character, SkillInfo skillInfo)
    {
        SkillInfo = skillInfo;
        HandleCastSkill(character);
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
        //AlkawaDebug.Log($"[Gameplay] - select skill {skillIndex}");
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
            //AlkawaDebug.Log($"Gameplay: Show skill range: {SkillInfo.range}");
        }
        
        //get character can be interact
        if (SkillInfo.damageType.HasFlag(DamageTargetType.Self))
        {
            characterManager.Characters.Add(characterManager.SelectedCharacter);
        }

        foreach (var cell in MapManager.SkillRange.Where(cell => cell.CellType == CellType.Character))
        {
            if (cell.Character.Type == characterManager.SelectedCharacter.Type 
                && SkillInfo.damageType.HasFlag(DamageTargetType.Team)
                )
            {
                characterManager.CharactersInRange.Add(cell.Character);
            }

            if (cell.Character.Type != characterManager.SelectedCharacter.Type 
                && SkillInfo.damageType.HasFlag(DamageTargetType.Enemies)
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

    public void ShowInfo(Character character)
    {
        ShowInfoCharacterParams = new CharacterParams()
        {
            Character = character,
            Skills = character.GetSkillInfos(characterManager.GetSkillType(character)),
        };
        // UIManager.Instance.ShowPopup(PopupType.ShowInfo);
    }

    #region Tutorial
    public void HandleEndSecondConversation()
    {
        // HUD.Instance.ShowHUD();
        characterManager.ShowAllHPBar();
        characterManager.SetMainCharacter();
    }
    
    #endregion
}
