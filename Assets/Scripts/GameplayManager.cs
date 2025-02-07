using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameplayManager : SingletonMonoBehavior<GameplayManager>
{
    [Title("Scriptable Objects")] [SerializeField] private LevelConfig levelConfig;

    [Title("Characters")] [SerializeField] private SerializableDictionary<CharacterType, Character> allCharacter = new();

    [Title("Tutorials")] [SerializeField] private GameObject tutorialPrefab;
    public bool IsTutorialLevel { get; set; }
    /*--------------------events-------------------------*/
    public event EventHandler OnLoadCharacterFinished;
    public event EventHandler<ShowInfoCharacterParameters> OnUpdateCharacterInfo;
    public event EventHandler OnSetMainCharacterFinished;
    public event EventHandler OnNewRound;

    /*---------------------------------------------------*/
    public MapManager MapManager {get; private set;}
    private readonly List<Character> _players = new();
    private readonly List<Character> _enemies = new();
    public List<Character> Characters { get; private set; } = new();
    public Character MainCharacter => Characters[CurrentPlayerIndex];
    public Character SelectedCharacter { get; set; }
    private Character _focusedCharacter;
    private Character _reactTarget;
    private HashSet<Character> _charactersInRange = new();

    public int CurrentRound { get; private set; }
    public int CurrentPlayerIndex { get; private set; }
    private bool IsRoundOfPlayer => MainCharacter.Type == Type.Player;
    private bool IsReact => MainCharacter.Type != SelectedCharacter.Type;
    private bool _canInteract;
    public LevelConfig LevelConfig => levelConfig;

    // new
    protected override void Awake()
    {
        base.Awake();
        UIManager.Instance.OpenMenu(MenuType.InGame);
        SetupTutorial();
    }

    private void Start()
    {
        StartNewGame();
    }

    protected override void UnRegisterEvents()
    {
        base.UnRegisterEvents();
        MapManager.OnLoadMapFinished -= OnLoadMapFinished;
    }

    #region Main

    private void StartNewGame()
    {
        CurrentRound = 0;
        if (!IsTutorialLevel) LoadMapGame();
    }

    public void LoadMapGame()
    {
        var go = Instantiate(levelConfig.mapPrefab, transform);
        MapManager = go.GetComponent<MapManager>();
        MapManager.OnLoadMapFinished += OnLoadMapFinished;
        MapManager.Initialize();
    }

    private void LoadCharacter()
    {
        Characters.Clear();
        _players.Clear();
        _enemies.Clear();
        SelectedCharacter = null;
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

                character.Initialize(MapManager.GetCell(point));
                if (IsTutorialLevel)
                {
                    character.HideHpBar();
                }
            }
        }

        SortCharacterBySpeed();
        SetMainCharacter();
        SetInteract(true);
        HandleNewRound();
        ShowLevelName();
    }
    
    public void SetMainCharacter()
    {
        if (TutorialManager.Instance != null
            && CurrentRound == 2
            && MainCharacter == Characters[0]
            && !TutorialManager.Instance.EndTuto)
        {
            MainCharacter.OnUnSelected();
            TutorialManager.Instance.OnNewRound();
        }
        else
        {
             if (MainCharacter.CharacterInfo.CurrentHp <= 0)
             {
                 HandleEndTurn();
             }
             else
             {
                 MainCharacter.SetMainCharacter();
                 SetSelectedCharacter(MainCharacter);
                 OnSetMainCharacterFinished?.Invoke(this, EventArgs.Empty);
             }
        }
    }

    private void SetSelectedCharacter(Character character)
    {
        SelectedCharacter?.OnUnSelected();
        SelectedCharacter = character;
        character.OnSelected();
        UpdateCharacterInfo();
        AlkawaDebug.Log(ELogCategory.GAMEPLAY, $"SetSelectedCharacter: {character.characterConfig.characterName}");
    }

    private void SetCharacterReact(Character character)
    {
        _reactTarget = SelectedCharacter;
        SetSelectedCharacter(character);
        AlkawaDebug.Log(ELogCategory.GAMEPLAY, $"SetCharacterReact: {character.characterConfig.characterName}");
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
                OnWaypointClicked(cell);
                break;
        }
    }
    
    public void HandleEndTurn(bool force = false)
    {
        if (!_canInteract && !force) return;
        MainCharacter.CharacterInfo.ResetBuffAfter();
        CurrentPlayerIndex++;
        if (CurrentPlayerIndex >= Characters.Count)
        {
            CurrentPlayerIndex = 0;
            HandleNewRound();
        }
        SetMainCharacter();
    }

    private void OnCharacterClicked(Cell cell)
    {
        if (_charactersInRange.Contains(cell.Character) && (!IsReact || cell.Character == _reactTarget))
        {
            HandleCastSkill(cell.Character);
            if (IsReact)
            {
                _reactTarget = null;
            }
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
                    if (cell.Character == SelectedCharacter)
                    {
                        UnSelectSkill();
                        // if (SelectedCharacter.IsMainCharacter) ShowMoveRange();
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
        var cellPath = MapManager.FindPath(SelectedCharacter.CharacterInfo.Cell, cell);
        SelectedCharacter.SetMovement(cellPath);
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
    
    public void ShowLevelName()
    {
        OnLoadCharacterFinished?.Invoke(this, EventArgs.Empty);
    }
    
    public Character GetCharacterByType(CharacterType characterType)
    {
        return Characters.FirstOrDefault(character => character.characterType == characterType);
    }
    
    private SkillInfo GetSkillInfo(int index)
    {
        return SelectedCharacter.CharacterInfo.GetSkillInfo(index, GetSkillType(SelectedCharacter));
    }
    
    private void HandleNewRound()
    {
        CurrentRound++;
        OnNewRound?.Invoke(this, EventArgs.Empty);
    }
    
    public void SetInteract(bool active)
    {
        _canInteract = active;
        AlkawaDebug.Log(ELogCategory.GAMEPLAY, $"Set Interact: {active}");
    }

    private void SortCharacterBySpeed()
    {
        Characters = Characters.OrderByDescending(c => c.CharacterInfo.Speed).ToList();
    }
    
    public void DestroyGameplay()
    {
        ((UI_Ingame)UIManager.Instance.CurrentMenu).HideAllUI();
        MapManager.DestroyMap();
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
            AlkawaDebug.Log(ELogCategory.GAMEPLAY, "GetFacingType: No opponents found.");
            return FacingType.Right;
        }

        var opponentPosition = nearestOpponent.transform.position;
        return characterPosition.x > opponentPosition.x ? FacingType.Left : FacingType.Right;
    }

    private bool CanMove(Cell cell)
    {
        return IsRoundOfPlayer && MainCharacter == SelectedCharacter && MapManager.CanMove(cell);
    }

    private ShowInfoCharacterParameters GetSelectedCharacterParams()
    {
        return new ShowInfoCharacterParameters
        {
            Character = SelectedCharacter,
            Skills = SelectedCharacter.GetSkillInfos(GetSkillType(SelectedCharacter))
        };
    }

    public SkillType GetSkillType(Character character)
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

    private void UpdateCharacterInfo()
    {
        OnUpdateCharacterInfo?.Invoke(this, GetSelectedCharacterParams());
    }
    
    #endregion

    #region Skills

    public void HandleSelectSkill(int skillIndex)
    {
        SelectedCharacter.HandleSelectSkill();
        // HideMoveRange();
        // UnSelectSkill();
        // if (_selectedCharacter.CharacterInfo.SkillInfo != GetSkillInfo(skillIndex))
        // {
        //     HideSkillRange();
        //     _selectedCharacter.CharacterInfo.SkillInfo = GetSkillInfo(skillIndex);
        //     if (_selectedCharacter.CharacterInfo.SkillInfo.isDirectionalSkill)
        //     {
        //         HandleDirectionalSkill();
        //     }
        //     else
        //     {
        //         HandleNonDirectionalSkill();
        //     }
        // }
        //
        // if (_selectedCharacter.CharacterInfo.SkillInfo != null) AlkawaDebug.Log(ELogCategory.GAMEPLAY, $"select skill {_selectedCharacter.CharacterInfo.SkillInfo.name}");
    }

    private void HandleCastSkill(Character character)
    {
        SelectedCharacter.HandleCastSkill();
        // HideMoveRange();
        // HideSkillRange();
        // _focusedCharacter = character;
        // _selectedCharacter.CharacterInfo.OnCastSkill(SkillInfo, SkillIndex.ActiveSkill1);
    }

    private void HandleCastSkill()
    {
        SelectedCharacter.CharacterInfo.OnCastSkill(SelectedCharacter.CharacterInfo.SkillInfo, SkillIndex.ActiveSkill1);
    }

    public void HandleCastSkill(Character character, SkillInfo skillInfo)
    {
        SelectedCharacter.CharacterInfo.SkillInfo = skillInfo;
        HandleCastSkill(character);
    }

    private void HandleNonDirectionalSkill()
    {
        HandleCastSkill();
    }
    
    private void HandleDirectionalSkill()
    {
        // show skill range
        if (SelectedCharacter.CharacterInfo.SkillInfo.range > 0)
        {
            //MapManager.ShowSkillRange(SelectedCharacter.CharacterInfo.Cell, SelectedCharacter.CharacterInfo.SkillInfo.range);
            AlkawaDebug.Log(ELogCategory.GAMEPLAY, $"Gameplay: Show skill range: {SelectedCharacter.CharacterInfo.SkillInfo.range}");
        }

        //get character can be interact
        if (SelectedCharacter.CharacterInfo.SkillInfo.damageType.HasFlag(DamageTargetType.Self))
        {
            Characters.Add(SelectedCharacter);
        }
        //
        // foreach (var cell in MapManager.SkillRange.Where(cell => cell.CellType == CellType.Character))
        // {
        //     if (cell.Character.Type == SelectedCharacter.Type
        //         && SelectedCharacter.CharacterInfo.SkillInfo.damageType.HasFlag(DamageTargetType.Team)
        //        )
        //     {
        //         _charactersInRange.Add(cell.Character);
        //     }
        //
        //     if (cell.Character.Type != SelectedCharacter.Type
        //         && SelectedCharacter.CharacterInfo.SkillInfo.damageType.HasFlag(DamageTargetType.Enemies)
        //        )
        //     {
        //         _charactersInRange.Add(cell.Character);
        //     }
        // }
    }

    public void OnCastSkillFinished()
    {
        UpdateCharacterInfo();
        if (SelectedCharacter.CharacterInfo.SkillInfo.isDirectionalSkill)
        {
            if (SelectedCharacter.CharacterInfo.SkillInfo.damageType.HasFlag(DamageTargetType.Enemies))
            {
                TryAttackEnemies(_focusedCharacter);
            }

            if (SelectedCharacter.CharacterInfo.SkillInfo.damageType.HasFlag(DamageTargetType.Team))
            {
                ApplyBuff();
            }
        }
        else
        {
            UnSelectSkill();
        }
    }
    
    private void ApplyBuff()
    {
        _focusedCharacter.CharacterInfo.ApplyBuff(SelectedCharacter.CharacterInfo.SkillInfo);
    }

    private bool TryAttackEnemies(Character focusedCharacter)
    {
        // check crit
        int damage = SelectedCharacter.CharacterInfo.SkillInfo.hasApplyDamage ? SelectedCharacter.CharacterInfo.BaseDamage : 0;
        var hitChange = SelectedCharacter.Roll.GetHitChange();
        var isCritical = SelectedCharacter.Roll.IsCritical(hitChange);
        if (!isCritical)
        {
            var dodge = focusedCharacter.CharacterInfo.Dodge;
            if (dodge > hitChange)
            {
                AlkawaDebug.Log(ELogCategory.GAMEPLAY,
                    $"[Gameplay] Né skill, dodge = {dodge} - hitChange = {hitChange}");
                focusedCharacter.CharacterInfo.OnDamageTaken(-1);
            }
            else
            {
                if (SelectedCharacter.CharacterInfo.SkillInfo.hasApplyDamage)
                {
                    damage += SelectedCharacter.Roll.RollDice(SelectedCharacter.CharacterInfo.SkillInfo.damageConfig);
                }

                // 
                // if (skillInfo.effectIndex != EffectIndex.None)
                {
                    // var debuffResistance = RollManager.Instance.GetDebuffResistance(focusedCharacter.Info.CharacterAttributes);
                    // if (debuffResistance < GameplayManager.EffectConstant
                    //         .effectData[_skillData.effectConfig.effectIndex].debuffResistance)
                    // {
                    //     focusedCharacter.Info.ApplyDeBuff(_skillData.effectConfig, SelectedCharacter);
                    //     Debug.Log($"[Gameplay] - Damage: {damage}");
                    // }
                }
                focusedCharacter.CharacterInfo.OnDamageTaken(damage);
            }
        }

        else
        {
            damage *= 2;
            focusedCharacter.CharacterInfo.OnDamageTaken(damage);
            //AlkawaDebug.Log($"Gameplay: Get Damage Taken {damage}");
        }

        return false;
    }

    public void HandleCharacterDie(Character character)
    {
        Characters.Remove(character);
        if (character is AICharacter)
        {
            _enemies.Remove(character);
            if (_enemies.Count == 0)
            {
                // Invoke(nameof(OnWin), 1f);
            }
        }
        else
        {
            _enemies.Remove(character);
            if (_enemies.Count == 0)
            {
                // Invoke(nameof(OnLose), 1f);
            }
        }
    }
    
    public void SetDamageTakenFinished()
    {
        if (_focusedCharacter.Type == Type.Player && MainCharacter.Type == Type.AI) // show react
        {
            UIManager.Instance.OpenPopup(PopupType.React);
        }
        else if (IsReact)
        {
            SelectedCharacter.ChangeState(ECharacterState.Idle);
            OnEndReact();
        }
    }

    private void OnEndReact()
    {
        HandleEndTurn(true);
    }
    
    private void UnSelectSkill()
    {
        SelectedCharacter.CharacterInfo.SkillInfo = null;
    }
    
    public List<Character> GetEnemiesInRange(Character character, int range)
    {
        var characters = MapManager.GetCharacterInRange(character.CharacterInfo.Cell, range);
        return characters.Where(c => c.Type != character.Type).ToList();
    }
    
    public void OnConFirmReact()
    {
        SetCharacterReact(_focusedCharacter);
        AlkawaDebug.Log(ELogCategory.GAMEPLAY,$"OnConFirmClick");
    }

    public void OnCancelReact()
    {
        OnEndReact();
        AlkawaDebug.Log(ELogCategory.GAMEPLAY,"OnCancelClick");
    }
    


    #endregion
    
    #region Tutorial

    private void SetupTutorial()
    {
        IsTutorialLevel = levelConfig.levelType == LevelType.Tutorial;
        if (IsTutorialLevel)
        {
            tutorialPrefab.SetActive(true);
        }
    }
    
    public void HandleEndSecondConversation()
    {
        ((UI_Ingame)UIManager.Instance.CurrentMenu).ShowAllUI();
        ShowAllHpBar();
        SetMainCharacter();
    }
    
    private void ShowAllHpBar()
    { 
         foreach (var character in Characters)
         {
             character.ShowHpBar();
         }
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