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

    [Title("Tutorials")] [SerializeField] private GameObject tutorialPrefab;

    public bool IsTutorialLevel { get; set; }

    /*--------------------events-------------------------*/
    public event EventHandler OnLoadCharacterFinished;
    public event EventHandler<ShowInfoCharacterParameters> OnUpdateCharacterInfo;
    public event EventHandler OnSetMainCharacterFinished;
    public event EventHandler OnNewRound;

    /*---------------------------------------------------*/
    public MapManager MapManager { get; private set; }
    private readonly List<Character> _players = new();
    private readonly List<Character> _enemies = new();
    public List<Character> Characters { get; private set; } = new();
    public Character MainCharacter => CurrentPlayerIndex >= Characters.Count ? null : Characters[CurrentPlayerIndex];

    public Character SelectedCharacter { get; set; }
    private Character _focusedCharacter;

    public int CurrentRound { get; private set; }
    public int CurrentPlayerIndex { get; private set; }
    private bool IsRoundOfPlayer => MainCharacter.Type == Type.Player;
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
        if (MapManager) MapManager.OnLoadMapFinished -= OnLoadMapFinished;
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
        Debug.Log("======================================================================");
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
            if (MainCharacter == null || MainCharacter.Info.IsDie)
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

    private void SetSelectedCharacter(Character character, IdleStateParams idleParams = null)
    {
        SelectedCharacter?.OnUnSelected();
        SelectedCharacter = character;
        SelectedCharacter.SetSelectedCharacter(idleParams);
        UpdateCharacterInfo();
        AlkawaDebug.Log(ELogCategory.GAMEPLAY, $"SetSelectedCharacter: {character.characterConfig.characterName}");
    }

    public void SetCharacterReact(Character character, DamageTakenParams damageTakenParams)
    {
        SetSelectedCharacter(character, new IdleStateParams
        {
            DamageTakenParams = damageTakenParams,
        });
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
        if (SelectedCharacter.IsReact)
        {
            SelectedCharacter.HandleEndReact();
        }
        else
        {
            if (SelectedCharacter)
                MainCharacter?.Info.ResetBuffAfter();
            CurrentPlayerIndex++;
            if (CurrentPlayerIndex >= Characters.Count)
            {
                CurrentPlayerIndex = 0;
                HandleNewRound();
            }

            SetMainCharacter();
        }
    }

    private void OnCharacterClicked(Cell cell)
    {
        if (SelectedCharacter.TryCastSkill(cell)) return;
        if (!IsRoundOfPlayer) return;
        if (cell.Character.Type == Type.AI)
        {
            SetSelectedCharacter(cell.Character);
        }
        else
        {
            if (cell.Character == SelectedCharacter)
            {
                SelectedCharacter.ShowMoveRange();
            }
            else
            {
                SetSelectedCharacter(cell.Character);
            }
        }
    }

    private void OnWaypointClicked(Cell cell)
    {
        if (SelectedCharacter.TryCastSkill(cell)) return;
        if (!CanMove()) return;
        SelectedCharacter.TryMoveToCell(cell);
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
        Characters = Characters.OrderByDescending(c => c.Info.Speed).ToList();
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
        var opponents = character.Type == Type.AI ? _players : _enemies;
        var nearestOpponent = Utils.FindNearestCharacter(character, opponents);

        if (nearestOpponent == null)
        {
            AlkawaDebug.Log(ELogCategory.GAMEPLAY, "GetFacingType: No opponents found.");
            return FacingType.Right;
        }

        var characterPosition = character.transform.position;
        var opponentPosition = nearestOpponent.transform.position;
        return characterPosition.x > opponentPosition.x ? FacingType.Left : FacingType.Right;
    }

    public Character GetNearestAlly(Character character)
    {
        var allies = character.Type == Type.AI ? _enemies : _players;
        return Utils.FindNearestCharacter(character, allies);
    }

    public Character GetNearestEnemy(Character character)
    {
        var enemies = character.Type == Type.AI ? _players : _enemies;
        return Utils.FindNearestCharacter(character, enemies);
    }


    private bool CanMove()
    {
        return IsRoundOfPlayer && MainCharacter == SelectedCharacter;
    }

    private ShowInfoCharacterParameters GetSelectedCharacterParams()
    {
        return new ShowInfoCharacterParameters
        {
            Character = SelectedCharacter,
            Skills = SelectedCharacter.GetSkillInfos(GetSkillTurnType(SelectedCharacter))
        };
    }

    public SkillTurnType GetSkillTurnType(Character character)
    {
        if (MainCharacter == null) return SkillTurnType.MyTurn;
        if (character == MainCharacter) return SkillTurnType.MyTurn;
        return character.Type == MainCharacter.Type ? SkillTurnType.TeammateTurn : SkillTurnType.EnemyTurn;
    }

    public void ShowInfo(Character character)
    {
        var showInfoParams = new ShowInfoCharacterParameters()
        {
            Character = character,
            Skills = character.GetSkillInfos(GetSkillTurnType(character)),
        };
        UIManager.Instance.OpenPopup(PopupType.ShowInfo, showInfoParams);
    }

    public void UpdateCharacterInfo()
    {
        OnUpdateCharacterInfo?.Invoke(this, GetSelectedCharacterParams());
    }

    #endregion

    #region Skills

    public void HandleSelectSkill(int skillIndex, Skill_UI skillUI)
    {
        SelectedCharacter.HandleSelectSkill(skillIndex, skillUI);
    }

    public void HandleCharacterDie(Character character)
    {
        Characters.Remove(character);
        if (character.Type == Type.AI)
        {
            _enemies.Remove(character);
            if (_enemies.Count == 0)
            {
                // Invoke(nameof(OnWin), 1f);
            }
        }
        else
        {
            _players.Remove(character);
            if (_players.Count == 0)
            {
                // Invoke(nameof(OnLose), 1f);
            }
        }
    }

    public List<Character> GetEnemiesInRange(Character character, int range, DirectionType directionType)
    {
        var characters = MapManager.GetCharacterInRange(character.Info.Cell, range, directionType);
        return characters.Where(c => c.Type != character.Type).ToList();
    }

    public void SwapPlayers(Character character1, Character character2)
    {
        var cell1 = character1.Info.Cell;
        var cell2 = character2.Info.Cell;
        cell1.Character = character2;
        cell2.Character = character1;

        character1.Info.Cell = cell2;
        character2.Info.Cell = cell1;
        
        character1.SetPosition();
        character2.SetPosition();
        
        cell1.HideFocus();
        cell2.ShowFocus();
        
        UpdateAllFacing();
    }

    public void UpdateAllFacing()
    {
        foreach (var character in Characters)
        {
            character.UpdateFacing();
        }
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