using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameplayManager : SingletonMonoBehavior<GameplayManager>
{
    [Title("Scriptable Objects")]
    [SerializeField] private LevelConfig levelConfig;  
    
    [Title("Characters")]
    [SerializeField] private SerializableDictionary<CharacterType, Character> allCharacter = new();
    /*--------------------events-------------------------*/
    public event EventHandler OnLoadCharacterFinished;
    public event EventHandler<ShowInfoCharacterParameters> OnSelectedCharacter;
    public event EventHandler OnSetMainCharacterFinished;
    
    /*---------------------------------------------------*/
    private MapManager _mapManager;
    private readonly List<Character> _players = new();
    private readonly List<Character> _enemies = new();
    public List<Character> Characters { get; private set; } = new();
    public Character MainCharacter => Characters[CurrentPlayerIndex];
    private Character _selectedCharacter;
    
    private HashSet<Character> _charactersInRange= new();
    
    public int CurrentRound { get; private set; }
    public int CurrentPlayerIndex { get; private set; }

    private bool IsRoundOfPlayer => MainCharacter.Type == Type.Player;

    private bool _canInteract;
    // new
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
        StartNewGame();
    }
    
    protected override void UnRegisterEvents()
    {
        base.UnRegisterEvents();
        _mapManager.OnLoadMapFinished -= OnLoadMapFinished;
    }

    #region Main 
    private void StartNewGame()
    {
        CurrentRound = 0;
        // HUD.Instance.SetLevelName(levelConfig.levelName);
        //
        // if (!IsTutorialLevel) 
        LoadMapGame();
    }
    
    public void LoadMapGame()
    {
        var go = Instantiate(levelConfig.mapPrefab, transform);
        _mapManager = go.GetComponent<MapManager>();
        _mapManager.OnLoadMapFinished += OnLoadMapFinished;
        _mapManager.Initialize();
    }
    
    private void LoadCharacter()
    {
        Characters.Clear();
        _players.Clear();
        _enemies.Clear();
        _selectedCharacter = null;
        foreach (var spawnPoint in levelConfig.spawnerConfig.spawnPoints)
        {
             foreach (var point in spawnPoint.Value.points)
             {
                 var go = Instantiate(allCharacter[spawnPoint.Key], transform);
                 var character = go.GetComponent<Character>();
                 Characters.Add(character);
                 switch (character.Type)
                 {
                     case Type.AI:
                         _enemies.Add(character);
                         break;
                     case Type.Player:
                         _players.Add(character);
                         break;
                 }
                 character.Initialize(_mapManager.GetCell(point));
                 // if (GPManager.IsTutorialLevel)
                 // {
                 //     character.HideHpBar();
                 // }
             }
        }
        
        SortCharacterBySpeed();
        SetMainCharacter();
        SetInteract(true);
        OnLoadCharacterFinished?.Invoke(this, EventArgs.Empty);
    }

    private void SetMainCharacter()
    {
        MainCharacter.SetMainCharacter();
        SetSelectedCharacter(MainCharacter);
        OnSetMainCharacterFinished?.Invoke(this, EventArgs.Empty);
    }

    private void SetSelectedCharacter(Character character)
    {
        _selectedCharacter?.OnUnSelected();
        _selectedCharacter = character;
        HideMoveRange();
        HideSkillRange();
        character.OnSelected();
        OnSelectedCharacter?.Invoke(this, GetSelectedCharacterParams());
        AlkawaDebug.Log(ELogCategory.GAMEPLAY,$"SetSelectedCharacter: {character.characterConfig.characterName}");
    }
    
    public void OnCellClicked(Cell cell)
    {
        if (!_canInteract) return;
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
        if (_charactersInRange.Contains(cell.Character))
        {
            HandleCastSkill(cell.Character);
        }
        else
        {
            if (IsRoundOfPlayer)
            {
                if (cell.Character.Type == Type.AI)
                {
                    SetSelectedCharacter(cell.Character);
                }
                else
                {
                    if (cell.Character == _selectedCharacter)
                    {
                        UnSelectSkill();
                        if (_selectedCharacter.IsMainCharacter) ShowMoveRange();
                    }
                    else
                    {
                        SetSelectedCharacter(cell.Character);
                    }
                }
            }   
        }
    }
    
    private void OnWaypointClicked(Cell cell)
    {
        if (!CanMove(cell)) return;
        var cellPath = _mapManager.FindPath(_selectedCharacter.characterInfo.Cell, cell);
        _mapManager.HideMoveRange();
        _selectedCharacter.MoveCharacter(cellPath);
    }
    
    #endregion
    #region Events

    private void OnLoadMapFinished(object sender, EventArgs e)
    {
        LoadCharacter();
        AlkawaDebug.Log(ELogCategory.GAMEPLAY, "Load Map Finished");
    }

    #endregion
    
    #region Sub

    public void SetInteract(bool active)
    {
        _canInteract = active;
        AlkawaDebug.Log(ELogCategory.GAMEPLAY, $"Set Interact: {active}");
    }
    private void SortCharacterBySpeed()
    { 
        Characters = Characters.OrderByDescending(c => c.characterInfo.Speed).ToList();
    }
    
    public void ShowMoveRange()
    {
        var range = _selectedCharacter.characterInfo.GetMoveRange();
        _mapManager.ShowMoveRange(_selectedCharacter.characterInfo.Cell, range);
        AlkawaDebug.Log(ELogCategory.GAMEPLAY,$"[{_selectedCharacter.characterConfig.characterName}] move Range: {range}");
    }
    
    private void HideMoveRange()
    {
         _mapManager.HideMoveRange();
    }

    private void HideSkillRange()
    {
        _mapManager.HideSkillRange();
        _charactersInRange.Clear();
    }

    public void DestroyGameplay()
    {
        _mapManager.DestroyMap();
        DestroyAllCharacters();
        StartNewGame();
    }

    private void DestroyAllCharacters()
    {
        foreach (var character in Characters)
        {
            character.DestroyCharacter();
        }
    }
    
    public FacingType GetFacingType(Character character)
    {
         if (character == null)
         {
             return FacingType.Right;
         }

         var characterPosition = character.transform.position;
         var nearestOpponent = Utils.FindNearestCharacter(character, character.Type == Type.AI ? _players : _enemies);
         if (nearestOpponent == null)
         {
             AlkawaDebug.Log(ELogCategory.GAMEPLAY,"GetFacingType: No opponents found.");
             return FacingType.Right;
         }

         var opponentPosition = nearestOpponent.transform.position;
         return characterPosition.x > opponentPosition.x ? FacingType.Left : FacingType.Right;
    }

    private bool CanMove(Cell cell)
    {
        return IsRoundOfPlayer && MainCharacter == _selectedCharacter && _mapManager.CanMove(cell);
    }
    
    private ShowInfoCharacterParameters GetSelectedCharacterParams()
    {
         return new ShowInfoCharacterParameters
         {
             Character = _selectedCharacter,
             Skills = _selectedCharacter.GetSkillInfos(GetSkillType(_selectedCharacter))
         };
    }
    
    private SkillType GetSkillType(Character character)
    {
         if (character == MainCharacter) return SkillType.InternalSkill;
         return character.Type == MainCharacter.Type ? SkillType.MovementSkill : SkillType.CombatSkill;
    }
    
    public void ShowInfo(Character character)
    {
        var showInfoParams = new ShowInfoCharacterParameters()
        {
            Character = character,
            Skills = character.GetSkillInfos(GetSkillType(character)),
        };
        UIManager.Instance.OpenPopup(PopupType.ShowInfo, showInfoParams);
    }
    
    #endregion
    // old
    [SerializeField] private GameObject tutorialPrefab;
    public SkillInfo SkillInfo { get; set; }
    
    public bool IsTutorialLevel;
    //public bool IsTutorialLevel => false;
    // Event
    public event EventHandler OnNewRound;

    public void HandleNewRound()
    {
        CurrentRound++;
        OnNewRound?.Invoke(this, EventArgs.Empty);
        // HUD.Instance.SetRound();
        //AlkawaDebug.Log($"NT - Gameplay: round {CurrentRound}");
    }
    

    private void HandleCastSkill(Character character)
    {
        // characterManager.HandleCastSkill(character);
    }

    public void HandleCastSkill(Character character, SkillInfo skillInfo)
    {
        SkillInfo = skillInfo;
        HandleCastSkill(character);
    }
    
    public void HandleEndTurn()
    {
        CurrentPlayerIndex++;
        // if (CharacterIndex >= characterManager.Characters.Count)
        // {
        //     CharacterIndex = 0;
        // }
        // characterManager.MainCharacter.characterInfo.ResetBuffAfter();
        // ResetAllChange();
        // characterManager.SetMainCharacter();
    }

    public void HandleSelectSkill(int skillIndex)
    {
        //AlkawaDebug.Log($"[Gameplay] - select skill {skillIndex}");
        // characterManager.HideMoveRange();
        UnSelectSkill();
        if (SkillInfo != GetSkillInfo(skillIndex))
        {
            // characterManager.HideSkillRange();
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
            //_mapManager.ShowSkillRange(characterManager.SelectedCharacter.characterInfo.Cell, SkillInfo.range);
            //AlkawaDebug.Log($"Gameplay: Show skill range: {SkillInfo.range}");
        }
        
        //get character can be interact
        if (SkillInfo.damageType.HasFlag(DamageTargetType.Self))
        {
            //characterManager.Characters.Add(characterManager.SelectedCharacter);
        }

        // foreach (var cell in _mapManager.SkillRange.Where(cell => cell.CellType == CellType.Character))
        // {
        //     if (cell.Character.Type == characterManager.SelectedCharacter.Type 
        //         && SkillInfo.damageType.HasFlag(DamageTargetType.Team)
        //         )
        //     {
        //         characterManager.CharactersInRange.Add(cell.Character);
        //     }
        //
        //     if (cell.Character.Type != characterManager.SelectedCharacter.Type 
        //         && SkillInfo.damageType.HasFlag(DamageTargetType.Enemies)
        //         )
        //     {
        //         characterManager.CharactersInRange.Add(cell.Character);
        //     }
        // }
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
        // characterManager.HideSkillRange();
    }

    private SkillInfo GetSkillInfo(int index)
    {
        return null;
        // return characterManager.GetSkillInfo(index);
    }

    private void ResetAllChange()
    {
        
    }
    
    #region Tutorial
    public void HandleEndSecondConversation()
    {
        // HUD.Instance.ShowHUD();
        // characterManager.ShowAllHPBar();
        // characterManager.SetMainCharacter();
    }
    
    #endregion
}

[Serializable]
public enum Type
{
    None,
    Player,
    AI,
}

public enum FacingType
{
    Left,
    Right,
}