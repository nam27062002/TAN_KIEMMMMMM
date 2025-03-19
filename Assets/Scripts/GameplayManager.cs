using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameplayManager : SingletonMonoBehavior<GameplayManager>
{
    #region Fields

    private readonly Dictionary<Type, List<Cell>> _characterDeath = new()
    {
        { Type.Player, new List<Cell>() },
        { Type.AI, new List<Cell>() }
    };

    private int _currentId = 0;
    
    #endregion

    #region Preload

    private void ClearAllData()
    {
        _characterDeath[Type.Player].Clear();
        _characterDeath[Type.AI].Clear();
        CurrentRound = 0;
        IsPauseGameInternal = false;
        cam.orthographicSize = levelConfig.cameraSize;
        charactersInConversation.Clear();
        Characters.Clear();
        Players.Clear();
        Enemies.Clear();
        SelectedCharacter = null;
        PreviousSelectedCharacter = null;
        _currentId = 0;
    }

    #endregion

    #region Characters

    private void LoadCharacter()
    {
        foreach (var character in charactersInConversation)
        {
            character.DestroyCharacter();
        }
        LoadCharactersFromConfig();
        ScheduleMainCharacterSetup();
        InitializeGameState();
    }

    private void LoadCharactersFromConfig()
    {
        if (_hasOverrideLevelConfig)
        {
            LoadFromSavedData();
            GameManager.Instance.saveIndex = -1;
        }
        else
        {
            LoadFromSpawnPoints();
            SortCharacterBySpeed();
        }
    }

    private void LoadFromSavedData()
    {
        var levelData = SaveLoadManager.Instance.levels[GameManager.Instance.saveIndex];

        foreach (var characterData in levelData.characterDatas)
        {
            var character = CreateCharacter(characterData.characterType, characterData.points, characterData.iD);
            ApplySavedCharacterState(character, characterData);
        }

        foreach (var characterData in levelData.characterDatas)
        {
            var character = GetCharacterByID(characterData.iD);
            if (character != null)
            {
                var effects = characterData.effectInfo.effects;
                foreach (var effect in effects)
                {
                    effect.Actor = character;
                    if (effect is BlockProjectile blockProjectile)
                    {
                        blockProjectile.targetCell = MapManager.Cells[blockProjectile.position];
                    }
                    
                    character.Info.ApplyEffect(effect);
                }
            }
            else
            {
                Debug.LogError($"không tìm thấy character có id = {characterData.iD}");
            }
        }
    }

    private Character GetCharacterByID(int id)
    {
        return Characters.FirstOrDefault(p => p.CharacterId == id);
    }

    private void LoadFromSpawnPoints()
    {
        foreach (var character in from spawnPoint in levelConfig.spawnerConfig.spawnPoints
                 from point in spawnPoint.Value.points
                 select CreateCharacter(spawnPoint.Key, point, _currentId))
        {
            _currentId++;
            HandleTutorialState(character);
        }
    }

    private Character CreateCharacter(CharacterType type, Vector2Int position, int iD)
    {
        var character = Instantiate(allCharacter[type], transform).GetComponent<Character>();
        character.Initialize(MapManager.GetCell(position), iD);
        Characters.Add(character);
        AddToAppropriateTeamList(character);
        return character;
    }

    private void AddToAppropriateTeamList(Character character)
    {
        switch (character.Type)
        {
            case Type.AI:
                Enemies.Add(character);
                break;
            case Type.Player:
                Players.Add(character);
                break;
        }
    }

    private void ApplySavedCharacterState(Character character, CharacterData data)
    {
        character.Info.CurrentHp = data.currentHp;
        character.Info.CurrentMp = data.currentMp;
        character.Info.OnHpChangedInvoke(0);
        character.Info.OnMpChangedInvoke(0);
    }

    public void HandleCharacterDeath(Character character,out Action callback)
    {
        callback = null;
        _characterDeath[character.Type].Add(character.Info.Cell);
        IsPauseGameInternal = false;
        SetInteract(true);
        Characters.Remove(character);
        if (character.Type == Type.AI)
        {
            HandleAIDeath(character);
        }
        else
        {
            HandlePlayerDeath(character);
        }
        HandleSpecialForLevel1(character);
        if (character.IsMainCharacter)
        {
            HandleEndTurn(0.3f, "Chết trong lượt chính");
        }
        else if (SelectedCharacter == character && character.Type != MainCharacter.Type)
        {
            Debug.Log($"[{character.characterConfig.characterName}] chết trong lượt counter => quay về lượt hiện tại");
            if (PreviousSelectedCharacter != null)
                callback = () => SetSelectedCharacter(PreviousSelectedCharacter);
            else if (MainCharacter != null)
                callback = () => SetSelectedCharacter(MainCharacter);
            else
                callback = () => HandleEndTurn("NONEEEEEEEEEEEEE");
        }
    }

    private void HandleAIDeath(Character character)
    {
        Enemies.Remove(character);
        if (Enemies.Count == 0)
        {
            PrepareForWinCondition();
        }
    }

    private void HandlePlayerDeath(Character character)
    {
        Players.Remove(character);
        if (Players.Count == 0)
        {
            PrepareForLoseCondition();
        }
    }

    private void PrepareForWinCondition()
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

    private void PrepareForLoseCondition()
    {
        ((UI_Ingame)UIManager.Instance.CurrentMenu).HideAllUI();
        IsPauseGameInternal = true;
        SetInteract(false);
        Invoke(nameof(OnLose), 1f);
    }

    #endregion

    #region Events

    public event EventHandler OnLoadCharacterFinished;

    protected override void RegisterEvents()
    {
        base.RegisterEvents();
    }

    protected override void UnRegisterEvents()
    {
        base.UnRegisterEvents();
        if (MapManager) MapManager.OnLoadMapFinished -= OnLoadMapFinished;
    }

    #endregion

    #region Sub

    public int GetCharacterDeathInRange(Character character, int range)
    {
        var cells = _characterDeath[character.Type == Type.Player ? Type.AI : Type.Player];
        int count = 0;
        foreach (var cell in cells)
        {
            var path = MapManager.FindShortestPath(cell, character.Info.Cell);
            if (path.Count <= range)
            {
                count++;
            }
        }

        return count;
    }

    #endregion

    //=================== OLD =====================================================

    [Title("Scriptable Objects")] [SerializeField]
    private List<LevelConfig> levelConfigs;

    public LevelType levelType;

    private LevelConfig levelConfig
    {
        get
        {
#if UNITY_EDITOR
            return levelConfigs[(int)levelType];
#else
            return levelConfigs[SaveLoadManager.currentLevel];
#endif
        }
    }


    [Title("Characters")] [SerializeField]
    private SerializableDictionary<CharacterType, Character> allCharacter = new();

    [Title("Tutorials")] [SerializeField] private GameObject tutorialPrefab;

    public Camera cam;
    public bool IsTutorialLevel { get; set; }

    /*--------------------events-------------------------*/
    public event EventHandler<ShowInfoCharacterParameters> OnUpdateCharacterInfo;
    public event EventHandler OnNewRound;
    public event EventHandler OnEndTurn;
    public event EventHandler OnRetry;

    /*---------------------------------------------------*/
    public MapManager MapManager { get; private set; }
    [ShowInInspector] public readonly List<Character> Players = new();
    [ShowInInspector] public readonly List<Character> Enemies = new();
    [ShowInInspector] public List<Character> Characters { get; private set; } = new();

    public List<Character> charactersInConversation = new();
    public Character MainCharacter => CurrentPlayerIndex >= Characters.Count ? Characters[0] : Characters[CurrentPlayerIndex];

    public Character SelectedCharacter { get; set; }
    public Character PreviousSelectedCharacter { get; set; }
    private Character _focusedCharacter;

    public int CurrentRound { get; private set; }
    public int CurrentPlayerIndex { get; private set; }
    private bool IsRoundOfPlayer => MainCharacter.Type == Type.Player;
    public bool CanInteract { get; set; }
    public LevelConfig LevelConfig => levelConfig;
    public bool IsPauseGameInternal = false;
    public bool IsReplay;

    private bool _hasOverrideLevelConfig;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            UIManager.Instance.OpenPopup(PopupType.Credit);
        }
    }

    #region Main

    private void StartNewGame()
    {
        ClearAllData();
        TryLoadFromSaveGame();
        if (!IsTutorialLevel)
            ShowStartConversation();
    }

    private void TryLoadFromSaveGame()
    {
        _hasOverrideLevelConfig = false;
        if (GameManager.Instance.saveIndex == -1) return;
        _hasOverrideLevelConfig = true;
        var levelData = SaveLoadManager.Instance.levels[GameManager.Instance.saveIndex];
        SaveLoadManager.currentLevel = (int)levelData.levelType;
        IsReplay = true;
    }

    private void ShowStartConversation()
    {
        if (levelConfig.startConversations is { Count: > 0 } && !CanSkipStartConversation())
        {
            ShowNextStartConversation(0);
        }
        else
        {
            LoadMapGame();
        }
    }

    private bool CanSkipStartConversation()
    {
#if UNITY_EDITOR
        return levelConfig.canSkipStartConversation;
#endif
        if (!IsReplay) return false;
        IsReplay = false;
        return true;
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
        SaveLoadManager.Instance.SetCurrentLevel((int)levelConfig.levelType);
        var go = Instantiate(levelConfig.mapPrefab, transform);
        MapManager = go.GetComponent<MapManager>();
        MapManager.OnLoadMapFinished += OnLoadMapFinished;
        MapManager.Initialize();
    }

    private void HandleTutorialState(Character character)
    {
        if (IsTutorialLevel)
        {
            character.HideHpBar();
        }
    }

    private void ScheduleMainCharacterSetup()
    {
        bool needsDelay = Characters.Any(p => p is CanSat);
        float delay = needsDelay ? 5f : 0f;

        if (delay > 0)
        {
            Invoke(nameof(SetMainCharacter), delay);
        }
        else
        {
            SetMainCharacter();
        }
    }

    private void InitializeGameState()
    {
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
                HandleEndTurn("Chết khi chuẩn bị đến lượt");
            }
            else
            {
                MainCharacter.SetMainCharacter();
                SetSelectedCharacter(MainCharacter);
                GameManager.Instance.OnMainCharacterChanged?.Invoke();
            }
        }
    }

    public void SetSelectedCharacter(Character character, IdleStateParams idleParams = null)
    {
        SelectedCharacter?.OnUnSelected();
        PreviousSelectedCharacter = SelectedCharacter;
        SelectedCharacter = character;
        
        SelectedCharacter.SetSelectedCharacter(idleParams);
        UpdateCharacterInfo();
        if (SelectedCharacter.Info.IsDie)
        {
            HandleEndTurn("Chết trong khi focus vào nhân vật (có thể do bị phản công)");
        }

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
        if (!CanInteract) return;
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

    private void HandleEndTurn(float delay, string message)
    {
        CoroutineDispatcher.Invoke(() => HandleEndTurn(message), delay);
    }

    public void HandleEndTurn(string message)
    {
        if (SelectedCharacter == null || IsPauseGameInternal) return;
        SetInteract(true);
        Debug.Log($"[{SelectedCharacter.characterConfig.characterName}]: End turn - {message}");
        if (SelectedCharacter.IsCounter)
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
        if (SelectedCharacter == null) return;
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
        CanInteract = active;
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
        SaveLoadManager.currentLevel++;
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

        var opponents = character.Type == Type.AI ? Players : Enemies;
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

    public FacingType GetFacingType(Character character, Character target)
    {
        if (character == null)
        {
            return FacingType.Right;
        }

        var characterPosition = character.transform.position;
        var opponentPosition = target.transform.position;
        return characterPosition.x > opponentPosition.x ? FacingType.Left : FacingType.Right;
    }

    public Character GetNearestAlly(Character character)
    {
        var allies = character.Type == Type.AI ? Enemies : Players;
        return Utils.FindNearestCharacter(character, allies);
    }

    public Character GetNearestEnemy(Character character)
    {
        var enemies = character.Type == Type.AI ? Players : Enemies;
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
            else if (character is ThietNhan && !Characters.Any(p => p is ThietNhan) &&
                     Characters.Any(p => p is RoiNguoi))
            {
                ((UI_Ingame)UIManager.Instance.CurrentMenu).HideAllUI();
                UIManager.Instance.OpenPopup(PopupType.Conversation, new ConversationPopupParameters()
                {
                    Conversation = levelConfig.special2Conversation.conversation,
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
            item.HandleDeath();
        }

        IsPauseGameInternal = true;
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
                        Players.Add(character);
                        break;
                }

                character.Initialize(MapManager.GetCell(point), _currentId);
                _currentId++;
            }
        }

        Invoke(nameof(SetCanInteract), 7f);
    }

    private void SetCanInteract()
    {
        IsPauseGameInternal = false;
        ((UI_Ingame)UIManager.Instance.CurrentMenu).ShowAllUI();
    }

    public List<Character> GetEnemiesInRange(Character character, int range, DirectionType directionType)
    {
        return GetCharactersInRangeFiltered(character, range, directionType, c => c.Type != character.Type);
    }

    public List<Character> GetTeammatesInRange(Character character, int range, DirectionType directionType)
    {
        return GetCharactersInRangeFiltered(character, range, directionType, c => c.Type == character.Type);
    }

    private List<Character> GetCharactersInRangeFiltered(Character character, int range, DirectionType directionType,
        Func<Character, bool> filter)
    {
        var characters = MapManager.GetCharacterInRange(character.Info.Cell, range, directionType);
        return characters.Where(filter).ToList();
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
        InitializeWinSequence(Players);
        StartCoroutine(WaitForCharactersExitCamera(Players, ProceedToNextLevel));
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
        float moveDistance = 30f;
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
        if (levelConfig.levelType == LevelType.Tutorial)
            NextLevel();
        else if (levelConfig.levelType == LevelType.Level1)
        {
            ShowAfterCredit();
        }
    }

    private void ShowAfterCredit()
    {
        UIManager.Instance.OpenPopup(PopupType.Credit);
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

    #region Save Load

    public void OnSave()
    {
        LevelData levelData = new LevelData();
        if (Characters == null) return;
        var chars = new List<Character>();
        for (int i = CurrentPlayerIndex; i < Characters.Count; i++)
        {
            chars.Add(Characters[i]);
        }

        for (int i = 0; i < CurrentPlayerIndex; i++)
        {
            chars.Add(Characters[i]);
        }
        
        foreach (var character in chars)
        {
            CharacterData characterData = new CharacterData
            {
                characterType = character.characterType,
                points = character.Info.Cell.CellPosition,
                currentHp = character.Info.CurrentHp,
                currentMp = character.Info.CurrentMp,
                iD = character.CharacterId,
                effectInfo = GetEffects(character),
            };
            levelData.characterDatas.Add(characterData);
        }

        levelData.SaveTime = DateTime.Now;
        levelData.levelType = levelConfig.levelType;
        SaveLoadManager.Instance.OnSave(SaveLoadManager.Instance.levels.Count, levelData);
        CoroutineDispatcher.Invoke(ShowNotification, 0.5f);

        return;
        
        IEffectInfo GetEffects(Character character)
        {
            var result = new List<EffectData>();
            foreach (var item in character.Info.EffectInfo.Effects)
            {
                item.characterId = character.CharacterId;
                if (item is BlockProjectile blockProjectile)
                {
                    blockProjectile.position = blockProjectile.targetCell.CellPosition;
                }
                result.Add(item);
            }
            return new IEffectInfo()
            {
                effects = result,
            };
        }
    }
    
    public void ShowNotification()
    {
        UIManager.Instance.OpenPopup(PopupType.Splash);
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