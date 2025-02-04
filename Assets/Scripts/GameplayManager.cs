using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameplayManager : SingletonMonoBehavior<GameplayManager>
{
    [Title("Scriptable Objects")] [SerializeField]
    private LevelConfig levelConfig;

    [Title("Characters")] [SerializeField]
    private SerializableDictionary<CharacterType, Character> allCharacter = new();

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
    private Character _focusedCharacter;
    public SkillInfo SkillInfo { get; set; }
    private HashSet<Character> _charactersInRange = new();

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
        AlkawaDebug.Log(ELogCategory.GAMEPLAY, $"SetSelectedCharacter: {character.characterConfig.characterName}");
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
        AlkawaDebug.Log(ELogCategory.GAMEPLAY,
            $"[{_selectedCharacter.characterConfig.characterName}] move Range: {range}");
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
            AlkawaDebug.Log(ELogCategory.GAMEPLAY, "GetFacingType: No opponents found.");
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
            _mapManager.ShowSkillRange(_selectedCharacter.characterInfo.Cell, SkillInfo.range);
            AlkawaDebug.Log(ELogCategory.GAMEPLAY, $"Gameplay: Show skill range: {SkillInfo.range}");
        }

        //get character can be interact
        if (SkillInfo.damageType.HasFlag(DamageTargetType.Self))
        {
            Characters.Add(_selectedCharacter);
        }

        foreach (var cell in _mapManager.SkillRange.Where(cell => cell.CellType == CellType.Character))
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
        // FocusedCharacter.characterInfo.OnDamageTaken(5, SetDamageTakenFinished);
        // check crit
        int damage = _selectedCharacter.characterInfo.BaseDamage;
        var hitChange = _selectedCharacter.Roll.GetHitChange();
        var isCritical = _selectedCharacter.Roll.IsCritical(hitChange);
        if (!isCritical)
        {
            var dodge = focusedCharacter.characterInfo.Dodge;
            if (dodge > hitChange)
            {
                AlkawaDebug.Log(ELogCategory.GAMEPLAY,
                    $"[Gameplay] Né skill, dodge = {dodge} - hitChange = {hitChange}");
                focusedCharacter.characterInfo.OnDamageTaken(0);
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

    public void SetDamageTakenFinished()
    {
        if (_focusedCharacter.Type == Type.Player && MainCharacter.Type == Type.AI) // react
        {
            Debug.Log("NT - React nereeeeeeeeeeeeeeeeeeeeeeeeeeee");
            // ReactMenu.Instance.Open();
        }
        else if (_selectedCharacter.characterInfo.IsReact)
        {
            OnEndReact();
        }
    }

    private void OnEndReact()
    {
        HandleEndTurn();
    }

    #endregion

    // old
    [SerializeField] private GameObject tutorialPrefab;

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

    public void HandleEndTurn()
    {
        CurrentPlayerIndex++;
        if (CurrentPlayerIndex >= Characters.Count)
        {
            CurrentPlayerIndex = 0;
        }

        MainCharacter.characterInfo.ResetBuffAfter();
        ResetAllChange();
        SetMainCharacter();
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
        HideSkillRange();
    }

    private SkillInfo GetSkillInfo(int index)
    {
        return _selectedCharacter.characterInfo.GetSkillInfo(index, GetSkillType(_selectedCharacter));
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