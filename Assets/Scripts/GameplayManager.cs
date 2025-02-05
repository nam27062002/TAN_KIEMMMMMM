using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.PlayerLoop;

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
    private Character _selectedCharacter;
    private Character _focusedCharacter;
    private Character _reactTarget;
    public SkillInfo SkillInfo { get; set; }
    private HashSet<Character> _charactersInRange = new();

    public int CurrentRound { get; private set; }
    public int CurrentPlayerIndex { get; private set; }
    private bool IsRoundOfPlayer => MainCharacter.Type == Type.Player;
    private bool IsReact => MainCharacter.Type != _selectedCharacter.Type;
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
        OnLoadCharacterFinished?.Invoke(this, EventArgs.Empty);
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
             if (MainCharacter.characterInfo.CurrentHp <= 0)
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
        _selectedCharacter?.OnUnSelected();
        _selectedCharacter = character;
        HideMoveRange();
        HideSkillRange();
        character.OnSelected();
        UpdateCharacterInfo();
        AlkawaDebug.Log(ELogCategory.GAMEPLAY, $"SetSelectedCharacter: {character.characterConfig.characterName}");
    }

    private void SetCharacterReact(Character character)
    {
        _reactTarget = _selectedCharacter;
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
        MainCharacter.characterInfo.ResetBuffAfter();
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
        var cellPath = MapManager.FindPath(_selectedCharacter.characterInfo.Cell, cell);
        MapManager.HideMoveRange();
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
    
    public Character GetCharacterByType(CharacterType characterType)
    {
        return Characters.FirstOrDefault(character => character.characterType == characterType);
    }
    
    private SkillInfo GetSkillInfo(int index)
    {
        return _selectedCharacter.characterInfo.GetSkillInfo(index, GetSkillType(_selectedCharacter));
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
        Characters = Characters.OrderByDescending(c => c.characterInfo.Speed).ToList();
    }

    public void ShowMoveRange()
    {
        var range = _selectedCharacter.characterInfo.GetMoveRange();
        MapManager.ShowMoveRange(_selectedCharacter.characterInfo.Cell, range);
        AlkawaDebug.Log(ELogCategory.GAMEPLAY,
            $"[{_selectedCharacter.characterConfig.characterName}] move Range: {range}");
    }

    private void HideMoveRange()
    {
        MapManager.HideMoveRange();
    }

    private void HideSkillRange()
    {
        Skill_UI.Selected?.highlightable.Unhighlight();
        MapManager.HideSkillRange();
        _charactersInRange.Clear();
    }

    public void DestroyGameplay()
    {
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
        return IsRoundOfPlayer && MainCharacter == _selectedCharacter && MapManager.CanMove(cell);
    }

    private ShowInfoCharacterParameters GetSelectedCharacterParams()
    {
        return new ShowInfoCharacterParameters
        {
            Character = _selectedCharacter,
            Skills = _selectedCharacter.GetSkillInfos(GetSkillType(_selectedCharacter))
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
        HideMoveRange();
        UnSelectSkill();
        if (SkillInfo != GetSkillInfo(skillIndex))
        {
            HideSkillRange();
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

        if (SkillInfo != null) AlkawaDebug.Log(ELogCategory.GAMEPLAY, $"select skill {SkillInfo.name}");
    }

    private void HandleCastSkill(Character character)
    {
        HideMoveRange();
        HideSkillRange();
        _focusedCharacter = character;
        _selectedCharacter.characterInfo.OnCastSkill(SkillInfo);
    }

    public void HandleCastSkill(Character character, SkillInfo skillInfo)
    {
        SkillInfo = skillInfo;
        HandleCastSkill(character);
    }

    private void HandleDirectionalSkill()
    {
        // show skill range
        if (SkillInfo.range > 0)
        {
            MapManager.ShowSkillRange(_selectedCharacter.characterInfo.Cell, SkillInfo.range);
            AlkawaDebug.Log(ELogCategory.GAMEPLAY, $"Gameplay: Show skill range: {SkillInfo.range}");
        }

        //get character can be interact
        if (SkillInfo.damageType.HasFlag(DamageTargetType.Self))
        {
            Characters.Add(_selectedCharacter);
        }

        foreach (var cell in MapManager.SkillRange.Where(cell => cell.CellType == CellType.Character))
        {
            if (cell.Character.Type == _selectedCharacter.Type
                && SkillInfo.damageType.HasFlag(DamageTargetType.Team)
               )
            {
                _charactersInRange.Add(cell.Character);
            }

            if (cell.Character.Type != _selectedCharacter.Type
                && SkillInfo.damageType.HasFlag(DamageTargetType.Enemies)
               )
            {
                _charactersInRange.Add(cell.Character);
            }
        }
    }

    public void OnCastSkillFinished()
    {
        UpdateCharacterInfo();
        if (SkillInfo.damageType.HasFlag(DamageTargetType.Enemies))
        {
            TryAttackEnemies(_focusedCharacter);
        }

        if (SkillInfo.damageType.HasFlag(DamageTargetType.Team))
        {
            ApplyBuff();
        }
    }
    
    private void ApplyBuff()
    {
        _focusedCharacter.characterInfo.ApplyBuff(SkillInfo);
    }

    private bool TryAttackEnemies(Character focusedCharacter)
    {
        // check crit
        int damage = SkillInfo.hasApplyDamage ? _selectedCharacter.characterInfo.BaseDamage : 0;
        var hitChange = _selectedCharacter.Roll.GetHitChange();
        var isCritical = _selectedCharacter.Roll.IsCritical(hitChange);
        if (!isCritical)
        {
            var dodge = focusedCharacter.characterInfo.Dodge;
            if (dodge > hitChange)
            {
                AlkawaDebug.Log(ELogCategory.GAMEPLAY,
                    $"[Gameplay] Né skill, dodge = {dodge} - hitChange = {hitChange}");
                focusedCharacter.characterInfo.OnDamageTaken(-1);
            }
            else
            {
                if (SkillInfo.hasApplyDamage)
                {
                    damage += _selectedCharacter.Roll.RollDice(SkillInfo.damageConfig);
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
                focusedCharacter.characterInfo.OnDamageTaken(damage);
            }
        }

        else
        {
            damage *= 2;
            focusedCharacter.characterInfo.OnDamageTaken(damage);
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
            _selectedCharacter.ChangeState(ECharacterState.Idle);
            OnEndReact();
        }
    }

    private void OnEndReact()
    {
        HandleEndTurn(true);
    }
    
    private void HandleNonDirectionalSkill()
    {
    }

    private void UnSelectSkill()
    {
        SkillInfo = null;
        HideSkillRange();
    }
    
    public List<Character> GetEnemiesInRange(Character character, int range)
    {
        var characters = MapManager.GetCharacterInRange(character.characterInfo.Cell, range);
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