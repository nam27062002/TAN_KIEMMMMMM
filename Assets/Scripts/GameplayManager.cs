using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameplayManager : SingletonMonoBehavior<GameplayManager>
{
    [Title("Scriptable Objects")] [SerializeField]
    private LevelConfig levelConfig;

    [Title("Characters")] [SerializeField]
    private SerializableDictionary<CharacterType, Character> allCharacter = new();

    [Title("Tutorials")] [SerializeField] private GameObject tutorialPrefab;

    public Camera cam;
    public bool IsTutorialLevel { get; set; }

    /*--------------------events-------------------------*/
    public event EventHandler OnLoadCharacterFinished;
    public event EventHandler<ShowInfoCharacterParameters> OnUpdateCharacterInfo;
    public event EventHandler OnSetMainCharacterFinished;
    public event EventHandler OnNewRound;
    public event EventHandler OnEndTurn;
    public event EventHandler OnRetry;

    /*---------------------------------------------------*/
    public MapManager MapManager { get; private set; }
    [ShowInInspector] private readonly List<Character> _players = new();
    [ShowInInspector] public readonly List<Character> Enemies = new();
    [ShowInInspector] public List<Character> Characters { get; private set; } = new();

    public List<Character> charactersInConversation = new();
    public Character MainCharacter => CurrentPlayerIndex >= Characters.Count ? null : Characters[CurrentPlayerIndex];

    public Character SelectedCharacter { get; set; }
    private Character _focusedCharacter;

    public int CurrentRound { get; private set; }
    public int CurrentPlayerIndex { get; private set; }
    private bool IsRoundOfPlayer => MainCharacter.Type == Type.Player;
    private bool _canInteract;
    public LevelConfig LevelConfig => levelConfig;
    public bool IsPauseGameInternal = false;

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
        IsPauseGameInternal = false;
        cam.orthographicSize = levelConfig.cameraSize;
        if (!IsTutorialLevel)
            ShowStartConversation();
    }

    private void ShowStartConversation()
    {
        if (levelConfig.startConversations is { Count: > 0 } && !levelConfig.canSkipStartConversation)
        {
            ShowNextStartConversation(0);
        }
        else
        {
            LoadMapGame();
        }
    }

    private void ShowNextStartConversation(int index)
    {
        if (index >= levelConfig.startConversations.Count)
        {
            LoadMapGame();
            return;
        }

        var conversationData = levelConfig.startConversations[index];
        UIManager.Instance.OpenPopup(PopupType.Conversation, new ConversationPopupParameters()
        {
            Conversation = conversationData.conversation,
            OnEndConversation = () => ShowNextStartConversation(index + 1)
        });
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
        foreach (var item in charactersInConversation)
        {
            item.DestroyCharacter();
        }

        charactersInConversation.Clear();
        Characters.Clear();
        _players.Clear();
        Enemies.Clear();
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
                        Enemies.Add(character);
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
        if (Characters.Any(p => p is CanSat))
        {
            Invoke(nameof(SetMainCharacter), 5f);
        }
        else
        {
            SetMainCharacter();
        }

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

    public void HandleEndTurn()
    {
        if (SelectedCharacter == null || IsPauseGameInternal) return;
        SetInteract(true);
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

        OnEndTurn?.Invoke(this, EventArgs.Empty);
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
        if (SelectedCharacter == null) return;
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
        foreach (var item in Characters)
        {
            item?.Info.IncreaseActionPointsValue();
        }

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
        DestroyAllCharacters();
        MapManager.DestroyMap();
        StartNewGame();
        OnRetry?.Invoke(this, EventArgs.Empty);
    }

    public void NextLevel()
    {
        levelConfig = levelConfig.nextLevel;
        DestroyGameplay();
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

        var opponents = character.Type == Type.AI ? _players : Enemies;
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
        var allies = character.Type == Type.AI ? Enemies : _players;
        return Utils.FindNearestCharacter(character, allies);
    }

    public Character GetNearestEnemy(Character character)
    {
        var enemies = character.Type == Type.AI ? _players : Enemies;
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
        if (SelectedCharacter == null) return;
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
        IsPauseGameInternal = false;
        SetInteract(true);
        Characters.Remove(character);
        if (character.Type == Type.AI)
        {
            Enemies.Remove(character);
            if (Enemies.Count == 0)
            {
                ((UI_Ingame)UIManager.Instance.CurrentMenu).HideAllUI();
                if (SelectedCharacter != null)
                {
                    SelectedCharacter.OnUnSelected();
                }

                IsPauseGameInternal = true;
                SetInteract(false);
                Invoke(nameof(ShowWinConversation), 1f);
            }
        }
        else
        {
            _players.Remove(character);
            if (_players.Count == 0)
            {
                ((UI_Ingame)UIManager.Instance.CurrentMenu).HideAllUI();
                IsPauseGameInternal = true;
                SetInteract(false);
                Invoke(nameof(OnLose), 1f);
            }
        }

        HandleSpecialForLevel1(character);
    }

    private void HandleSpecialForLevel1(Character character)
    {
        if (levelConfig.levelType == LevelType.Level1)
        {
            if (character is RoiNguoi && Characters.Any(p => p is ThietNhan))
            {
                ((UI_Ingame)UIManager.Instance.CurrentMenu).HideAllUI();
                var characters = Characters.OfType<ThietNhan>().Cast<Character>().ToList();
                InitializeWinSequence(characters);
                StartCoroutine(WaitForCharactersExitCamera(characters));
                UIManager.Instance.OpenPopup(PopupType.Conversation, new ConversationPopupParameters()
                {
                    Conversation = levelConfig.special1Conversation.conversation,
                    OnEndConversation = OnEndSpecificConversation,
                });
            }
        }
    }

    private void OnEndSpecificConversation()
    {
        SpawnSpecialEnemy();
        foreach (var item in Characters.OfType<ThietNhan>().ToList())
        {
            item.OnDie();
        }
    }

    private void SpawnSpecialEnemy()
    {
        foreach (var spawnPoint in levelConfig.specialSpawnerConfig.spawnPoints)
        {
            foreach (var point in spawnPoint.Value.points)
            {
                var go = Instantiate(allCharacter[spawnPoint.Key], transform);
                var character = go.GetComponent<Character>();
                Characters.Add(character);
                switch (character.Type)
                {
                    case Type.AI:
                        Enemies.Add(character);
                        break;
                    case Type.Player:
                        _players.Add(character);
                        break;
                }

                character.Initialize(MapManager.GetCell(point));
            }
        }
        Invoke(nameof(SetCanInteract), 5f);
    }

    private void SetCanInteract()
    {
        ((UI_Ingame)UIManager.Instance.CurrentMenu).ShowAllUI();
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
            character?.UpdateFacing();
        }
    }

    private void OnLose()
    {
        UIManager.Instance.OpenPopup(PopupType.Lose);
    }

    private void ShowWinConversation()
    {
        if (levelConfig.winConversations is { Count: > 0 })
        {
            ShowNextConversation(0);
        }
        else
        {
            OnWin();
        }
    }

    private void ShowNextConversation(int index)
    {
        if (index >= levelConfig.winConversations.Count)
        {
            OnWin();
            return;
        }

        var conversationData = levelConfig.winConversations[index];
        UIManager.Instance.OpenPopup(PopupType.Conversation, new ConversationPopupParameters()
        {
            Conversation = conversationData.conversation,
            OnEndConversation = () => ShowNextConversation(index + 1)
        });
    }

    private void OnWin()
    {
        InitializeWinSequence(_players);
        StartCoroutine(WaitForCharactersExitCamera(_players, ProceedToNextLevel));
    }

    private Sequence _winSequence;
    
    private void InitializeWinSequence(List<Character> characters)
    {
        _winSequence = DOTween.Sequence();
        foreach (var player in characters)
        {
            SetupCharacterMovement(player);
        }
    }

    private void SetupCharacterMovement(Character character)
    {
        character.SetFacing(FacingType.Right);
        character.AnimationData.PlayAnimation(AnimationParameterNameType.MoveRight);

        const float moveDuration = 8f;
        float moveDistance = 20f;
        Vector3 targetPosition = character.transform.position + Vector3.right * moveDistance;

        _winSequence.Join(character.transform
            .DOMoveX(targetPosition.x, moveDuration)
            .SetEase(Ease.Linear));
    }

    private IEnumerator WaitForCharactersExitCamera(List<Character> characters, Action action = null)
    {
        yield return WaitUntilAllCharactersExitCamera(characters);
        CleanupSequence();
        action?.Invoke();
    }

    private IEnumerator WaitUntilAllCharactersExitCamera(List<Character> characters)
    {
        Camera mainCamera = GetMainCamera();
        if (mainCamera == null) yield break;

        while (!AreAllCharactersOutOfCamera(characters, mainCamera))
        {
            yield return null;
        }
    }

    private Camera GetMainCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
        }

        return mainCamera;
    }

    private bool AreAllCharactersOutOfCamera(List<Character> characters, Camera camera)
    {
        foreach (var character in characters)
        {
            if (character == null)
            {
                CleanupSequence();
                return true;
            }
            if (IsCharacterVisible(character.transform.position, camera))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsCharacterVisible(Vector3 worldPosition, Camera camera)
    {
        Vector3 adjustedPosition = worldPosition - Vector3.right * 0.5f;
        Vector3 viewportPoint = camera.WorldToViewportPoint(adjustedPosition);
        return IsViewportPointVisible(viewportPoint);
    }

    private bool IsViewportPointVisible(Vector3 viewportPoint)
    {
        return viewportPoint.x.IsBetween(0f, 1f)
               && viewportPoint.y.IsBetween(0f, 1f);
    }


    private void CleanupSequence()
    {
        if (_winSequence == null || !_winSequence.IsActive()) return;

        _winSequence.Kill();
        _winSequence = null;
    }

    private void ProceedToNextLevel()
    {
        // UIManager.Instance.OpenPopup(PopupType.Win);
        NextLevel();
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