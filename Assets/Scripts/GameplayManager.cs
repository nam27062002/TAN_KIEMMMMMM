using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameplayManager : SingletonMonoBehavior<GameplayManager>
{
    #region Fields
    public bool CanShowSkipTutorial { get; set; } = false;
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
        charactersInConversation.Clear();
        Characters.Clear();
        Players.Clear();
        Enemies.Clear();
        SelectedCharacter = null;
        PreviousSelectedCharacter = null;
        _currentId = 0;
        _process = false;
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
        // Đặt flag trước khi tạo nhân vật
        CanSat.IsLoadingFromSave = true;
        
        var levelData = SaveLoadManager.Instance.levels[GameManager.Instance.saveIndex];
        
        // Tách danh sách nhân vật thường và bóng
        var normalCharacters = levelData.characterDatas.Where(c => !c.isShadow).ToList();
        var shadowCharacters = levelData.characterDatas.Where(c => c.isShadow).ToList();

        // Tìm các CanSat trước và đánh dấu loadedFromSave = true
        Dictionary<CharacterType, bool> isCanSatType = new Dictionary<CharacterType, bool>();
        foreach (var charData in normalCharacters)
        {
            if (allCharacter[charData.characterType].GetComponent<CanSat>() != null)
            {
                isCanSatType[charData.characterType] = true;
            }
        }

        // Tạo các nhân vật thường
        Dictionary<int, Character> idToCharacter = new Dictionary<int, Character>();
        
        // Lưu ID của nhân vật đầu tiên để chỉ giữ MoveAmount cho nhân vật này
        int firstCharacterId = normalCharacters.Count > 0 ? normalCharacters[0].iD : -1;
        
        foreach (var characterData in normalCharacters)
        {
            bool isFirstCharacter = (characterData.iD == firstCharacterId);
            
            // Nếu là CanSat, cần setup trước khi Initialize
            if (isCanSatType.ContainsKey(characterData.characterType) && isCanSatType[characterData.characterType])
            {
                var canSatGO = Instantiate(allCharacter[characterData.characterType], transform);
                var canSat = canSatGO.GetComponent<CanSat>();
                canSat.loadedFromSave = true;
                
                canSat.Initialize(MapManager.GetCell(characterData.points), characterData.iD);
                ApplySavedCharacterState(canSat, characterData, isFirstCharacter);
                
                Characters.Add(canSat);
                AddToAppropriateTeamList(canSat);
                idToCharacter[canSat.CharacterId] = canSat;
            }
            else
            {
                // Các nhân vật khác xử lý bình thường
                var character = CreateCharacter(characterData.characterType, characterData.points, characterData.iD);
                ApplySavedCharacterState(character, characterData, isFirstCharacter);
                idToCharacter[character.CharacterId] = character;
            }
        }

        // Tạo các bóng
        foreach (var shadowData in shadowCharacters)
        {
            GameObject shadowPrefab = null;
            CharacterType shadowType = shadowData.shadowType;
            
            // Kiểm tra và lấy prefab của bóng từ CanSat owner
            if (idToCharacter.TryGetValue(shadowData.ownerID, out var owner) && owner is CanSat canSat)
            {
                // Dùng enum để so sánh
                if (shadowType == CharacterType.Dancer)
                    shadowPrefab = canSat.dancerPrefab;
                else if (shadowType == CharacterType.Assassin)
                    shadowPrefab = canSat.assassinPrefab;
                
                if (shadowPrefab == null)
                {
                    Debug.LogError($"Không thể tìm thấy prefab cho bóng loại {shadowType}");
                    continue;
                }
                
                // Tạo bóng
                var go = Instantiate(shadowPrefab);
                Shadow shadow = null;
                
                // Dùng enum để kiểm tra loại shadow
                if (shadowType == CharacterType.Dancer)
                    shadow = go.GetComponent<Dancer>();
                else if (shadowType == CharacterType.Assassin)
                    shadow = go.GetComponent<Assassin>();
                    
                if (shadow == null)
                {
                    Debug.LogError($"Không thể tạo bóng loại {shadowType}");
                    Destroy(go);
                    continue;
                }
                    
                shadow.Initialize(MapManager.GetCell(shadowData.points), shadowData.iD);
                shadow.Info.CurrentHp = shadowData.currentHp;
                shadow.Info.CurrentMp = shadowData.currentMp;
                shadow.Info.MoveAmount = shadowData.moveAmount;
                
                // Bóng không phải nhân vật chính nên luôn reset MoveAmount
                shadow.Info.IsFirstRoundAfterLoad = false;
                
                shadow.Info.OnHpChangedInvoke(0);
                shadow.Info.OnMpChangedInvoke(0);
                
                // Gán owner
                shadow.owner = canSat;
                
                // Gọi SetShadow với enum
                canSat.SetShadow(shadow, shadowType);
                
                // Xử lý effects
                foreach (var effect in shadowData.effectInfo.effects)
                {
                    effect.Actor = shadow;
                    if (effect is BlockProjectile blockProjectile)
                    {
                        blockProjectile.targetCell = MapManager.Cells[blockProjectile.position];
                    }

                    shadow.Info.ApplyEffect(effect);
                }
                
                // Khôi phục Action Points cho bóng
                if (shadowData.actionPoints != null && shadowData.actionPoints.Count > 0)
                {
                    shadow.Info.ActionPoints = new List<int>(shadowData.actionPoints);
                    Debug.Log($"[{shadow.characterConfig.characterName}] Khôi phục Action Points: {string.Join(", ", shadowData.actionPoints)}");
                }
            }
            else
            {
                Debug.LogError($"Không tìm thấy owner với ID {shadowData.ownerID} cho bóng loại {shadowType}");
            }
        }

        // Áp dụng effects cho nhân vật thường
        foreach (var characterData in normalCharacters)
        {
            var character = idToCharacter[characterData.iD];
            var effects = characterData.effectInfo.effects;
            foreach (var effect in effects)
            {
                effect.Actor = character;
                
                // Gọi OnAfterLoad cho tất cả hiệu ứng
                effect.OnAfterLoad(MapManager);
                
                character.Info.ApplyEffect(effect);
            }
            
            // Đảm bảo cập nhật visual cho shield nếu có
            if (character.Info.ShieldEffectData != null)
            {
                character.Info.UpdateShieldVisual();
            }
            
            // Khôi phục Action Points cho nhân vật thường
            if (characterData.actionPoints != null && characterData.actionPoints.Count > 0)
            {
                character.Info.ActionPoints = new List<int>(characterData.actionPoints);
                Debug.Log($"[{character.characterConfig.characterName}] Khôi phục Action Points: {string.Join(", ", characterData.actionPoints)}");
            }

            // Khôi phục shield cho Hoắc Liên Hương
            if (character is HoacLienHuong hlh && characterData.shieldCellPosition.HasValue)
            {
                var shieldCell = MapManager.GetCell(characterData.shieldCellPosition.Value);
                if (shieldCell != null)
                {
                    // Tạo shield và cập nhật visual
                    shieldCell.SetShield(character.Type, 3);
                    hlh.CurrentShield = shieldCell;
                }
            }
        }

        // Sau khi áp dụng tất cả hiệu ứng
        foreach (var character in Characters)
        {
            character.Info.InitializeEffectVisuals();
        }

        CurrentRound = levelData.currentRound;
        Debug.Log($"Loaded game from save slot {GameManager.Instance.saveIndex}, current round: {CurrentRound}");

        // Đặt lại flag sau khi đã tạo xong
        CanSat.IsLoadingFromSave = false;
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

    private void ApplySavedCharacterState(Character character, CharacterData data, bool isFirstCharacter = false)
    {
        character.Info.CurrentHp = data.currentHp;
        character.Info.CurrentMp = data.currentMp;
        character.Info.MoveAmount = data.moveAmount;
        
        // Khôi phục IncreasedDamageTaken và maxMoveRange
        character.Info.IncreasedDamageTaken = data.increasedDamageTaken;
        character.Info.Attributes.maxMoveRange = data.maxMoveRange;
        
        // Thêm code để khôi phục Action Points
        if (data.actionPoints != null && data.actionPoints.Count > 0)
        {
            character.Info.ActionPoints = new List<int>(data.actionPoints);
        }
        
        // Chỉ đặt IsFirstRoundAfterLoad = true cho nhân vật đầu tiên
        character.Info.IsFirstRoundAfterLoad = isFirstCharacter;
        
        if (isFirstCharacter)
            Debug.Log($"[{character.characterConfig.characterName}] Đây là nhân vật chính đầu tiên, sẽ giữ nguyên MoveAmount = {data.moveAmount}");
        else
            Debug.Log($"[{character.characterConfig.characterName}] Không phải nhân vật chính đầu tiên, sẽ reset MoveAmount thành 0 khi đến lượt");
            
        character.Info.OnHpChangedInvoke(0);
        character.Info.OnMpChangedInvoke(0);

        // Khôi phục giá trị VenomousParasite nếu là Đoàn Gia Linh
        if (character.characterType == CharacterType.DoanGiaLinh)
        {
            var doanGiaLinh = character as DoanGiaLinh;
            if (doanGiaLinh != null)
            {
                doanGiaLinh.SetVenomousParasite(data.venomousParasite);
                //AlkawaDebug.Log(ELogCategory.SAVE, $"[LoadGame] Khôi phục Độc Trùng của Đoàn Gia Linh: {data.venomousParasite}");
            }
        }
    }

    public void HandleCharacterDeath(Character character, out Action callback)
    {
        if (character == null)
        {
            Debug.LogException(new System.NullReferenceException("character is null in HandleCharacterDeath"));
            callback = null;
            return;
        }
        callback = null;
        _characterDeath[character.Type].Add(character.Info.Cell);
        IsPauseGameInternal = false;
        SetInteract(true);
        int characterIndex = Characters.IndexOf(character);
        Characters.Remove(character);
        if (characterIndex < CurrentPlayerIndex)
        {
            CurrentPlayerIndex--;
        }

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
            HandleEndTurn( "Chết trong lượt chính");
        }
        else if (SelectedCharacter == character)
        {
            Debug.Log($"[DEBUG] SelectedCharacter == character: {true}");
            Debug.Log($"[DEBUG] character.Type != MainCharacter.Type: {character.Type != MainCharacter.Type}");
            Debug.Log($"[DEBUG] character.Type == Type.AI: {character.Type == Type.AI}");

            if (character.Type != MainCharacter.Type)
            {
                Debug.Log($"[{character.characterConfig.characterName}] chết trong lượt counter => quay về lượt hiện tại");
                if (PreviousSelectedCharacter != null)
                    callback = () => SetSelectedCharacter(PreviousSelectedCharacter);
                else if (MainCharacter != null)
                    callback = () => SetSelectedCharacter(MainCharacter);
                else
                    callback = () => HandleEndTurn("NONEEEEEEEEEEEEE");
            }
            else
            {
                Debug.Log($"[WARNING] AI chết nhưng không quay về lượt hiện tại - Character: {character.characterConfig.characterName}, Type: {character.Type}, MainType: {MainCharacter.Type}");
            }
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
        CanShowSkipTutorial = false;
        SetMainCell(null);
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

    [Title("Scriptable Objects")]
    [SerializeField]
    private List<LevelConfig> levelConfigs;

    [SerializeField] private LevelType levelType;

    private LevelConfig levelConfig => levelConfigs[(int)levelType];


    [Title("Characters")]
    [SerializeField]
    private SerializableDictionary<CharacterType, Character> allCharacter = new();

    [Title("Tutorials")][SerializeField] private GameObject tutorialPrefab;

    public Camera cam;
    public bool IsTutorialLevel { get; set; }

    /*--------------------events-------------------------*/
    public event EventHandler<ShowInfoCharacterParameters> OnUpdateCharacterInfo;
    public event EventHandler OnNewRound;
    public event EventHandler OnEndTurn;
    public event EventHandler OnRetry;
    public event EventHandler<Cell> OnSetMainCharacter;
    /*---------------------------------------------------*/
    public MapManager MapManager { get; private set; }
    public GameObject mapPrefab;
    [ShowInInspector] public readonly List<Character> Players = new();
    [ShowInInspector] public readonly List<Character> Enemies = new();
    public int CurrentPlayerIndex;
    public List<Character> Characters = new();

    public List<Character> charactersInConversation = new();
    [ShowInInspector] public Character MainCharacter { get; set; }

    [ShowInInspector] public Character SelectedCharacter { get; set; }
    [ShowInInspector] public Character PreviousSelectedCharacter { get; set; }
    private Character _focusedCharacter;

    public int CurrentRound { get; private set; }

    private bool IsRoundOfPlayer => MainCharacter.Type == Type.Player;
    public bool CanInteract { get; set; }
    public LevelConfig LevelConfig => levelConfig;
    public bool IsPauseGameInternal = false;

    private bool _hasOverrideLevelConfig;

    // new
    protected override void Awake()
    {
        base.Awake();
        UIManager.Instance.OpenMenu(MenuType.InGame);

        // --- Thêm code: Đọc trạng thái Replay từ GameManager ---
        if (GameManager.Instance != null && GameManager.Instance.IsReplaying && GameManager.Instance.LevelToReplay.HasValue)
        {
            levelType = GameManager.Instance.LevelToReplay.Value;
            AlkawaDebug.Log(ELogCategory.GAMEPLAY, $"GameplayManager started in Replay mode for level: {levelType}");
            // Đặt lại flag trong GameManager sau khi đã đọc
            // GameManager.Instance.LevelToReplay = null; // Không reset ở đây, có thể cần dùng lại
            // GameManager.Instance.IsReplaying = false;
        }
        else
        {
            AlkawaDebug.Log(ELogCategory.GAMEPLAY, "GameplayManager started normally.");
        }
        // ------------------------------------------------------
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
        else if (Input.GetKeyDown(KeyCode.A))
        {
            DOTween.KillAll();
            OnWin();
        }
    }

    #region Main

    private void StartNewGame()
    {
        ClearAllData();
        TryLoadFromSaveGame();
        cam.orthographicSize = levelConfig.cameraSize;
        SetupTutorial();
        if (!IsTutorialLevel)
            ShowStartConversation();
    }

    private void TryLoadFromSaveGame()
    {
        _hasOverrideLevelConfig = false;
        // --- Sửa code: Dùng saveIndex từ GameManager ---
        if (GameManager.Instance.saveIndex == -1) return;
        _hasOverrideLevelConfig = true;
        var levelData = SaveLoadManager.Instance.levels[GameManager.Instance.saveIndex];
        levelType = levelData.levelType; // Ghi đè levelType nếu load từ save
        // IsReplay = true; // Không dùng biến này nữa
        // -------------------------------------------
    }

    private void ShowStartConversation()
    {
        // --- Sửa code: Kiểm tra GameManager.IsReplaying ---
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
        if (!GameManager.Instance.IsReplaying) 
        {
            return _hasOverrideLevelConfig; 
        }
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
        if (mapPrefab != null)
        {
            Destroy(mapPrefab);
        }
        mapPrefab = Instantiate(levelConfig.mapPrefab.gameObject, transform);
        MapManager = mapPrefab.GetComponent<MapManager>();
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
        // Nếu load từ save, gọi SetMainCharacter ngay lập tức
        if (_hasOverrideLevelConfig)
        {
            SetMainCharacter();
            return;
        }
        
        // Chỉ có độ trễ khi tạo mới game
        bool needsDelay = Characters.Any(p => p is CanSat);
        float delay = needsDelay ? 5f : 0f;

        if (delay > 0)
        {
            Debug.Log("Đang đợi CanSat khởi tạo các bóng...");
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
        
        // Chỉ gọi HandleNewRound nếu không load từ save
        if (!_hasOverrideLevelConfig)
        {
            HandleNewRound();
        }
        else
        {
            // Chỉ thông báo round hiện tại mà không tăng lên
            OnNewRound?.Invoke(this, EventArgs.Empty);
        }
        
        ShowLevelName();
    }

    public void SetMainCharacter()
    {
        Debug.Log("======================================================================");
        MainCharacter = CurrentPlayerIndex >= Characters.Count ? Characters[0] : Characters[CurrentPlayerIndex];

        // Reset có thể sử dụng skill khi bắt đầu lượt chính - di chuyển lên trước
        if (MainCharacter != null)
        {
            MainCharacter.CanUseSkill = true;
        }

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
                if (MainCharacter != null)
                    OnSetMainCharacter?.Invoke(this, MainCharacter.Info.Cell);
                SetSelectedCharacter(MainCharacter);
                
                // Hiển thị tầm di chuyển sau khi load từ save
                if (_hasOverrideLevelConfig && MainCharacter.Type == Type.Player)
                {
                    AlkawaDebug.Log(ELogCategory.EDITOR, $"Load từ save - Hiển thị tầm di chuyển cho {MainCharacter.characterConfig.characterName}");
                    // Đảm bảo hiển thị tầm di chuyển ngay khi load từ save
                    MainCharacter.ShowMoveRange();
                }
                
                GameManager.Instance.OnMainCharacterChanged?.Invoke();
            }
        }
        UpdateCharacterInfo();
    }

    public void SetMainCell(Cell cell)
    {
        OnSetMainCharacter?.Invoke(this, cell);
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
        SetInteract(false);
        SetSelectedCharacter(character, new IdleStateParams
        {
            DamageTakenParams = damageTakenParams,
        });
        UpdateCharacterInfo();
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

    public void HandleEndTurn(float delay, string message)
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
        foreach (var character in charactersInConversation)
        {
            character.DestroyCharacter();
        }
        charactersInConversation.Clear();
        ((UI_Ingame)UIManager.Instance.CurrentMenu).HideAllUI();
        DestroyAllCharacters();
        MapManager?.DestroyMap();
        StartNewGame();
        OnRetry?.Invoke(this, EventArgs.Empty);
    }

    public void NextLevel()
    {
        IsTutorialLevel = false;
        // SaveLoadManager.currentLevel++;
        levelType++;
        SaveLoadManager.Instance.IsFinishedTutorial = true;
        DestroyGameplay();
    }

    public void DestroyAllCharacters()
    {
        // Đầu tiên, lưu các nhân vật CanSat và hủy bóng trước
        var canSatCharacters = Characters.OfType<CanSat>().ToList();
        foreach (var canSat in canSatCharacters)
        {
            // Hủy dancer trước
            if (canSat.dancer != null)
            {
                // Kiểm tra xem dancer gameObject có tồn tại không trước khi hủy
                if (canSat.dancer.gameObject != null)
                {
                    canSat.dancer.DestroyCharacter();
                }
                // Luôn đặt về null sau khi xử lý
                canSat.dancer = null;
            }

            // Hủy assassin trước
            if (canSat.assassin != null)
            {
                // Kiểm tra xem assassin gameObject có tồn tại không trước khi hủy
                if (canSat.assassin.gameObject != null)
                {
                    canSat.assassin.DestroyCharacter();
                }
                // Luôn đặt về null sau khi xử lý
                canSat.assassin = null;
            }
        }

        // Sau đó hủy các nhân vật còn lại
        // Tạo bản sao danh sách để tránh lỗi khi sửa đổi danh sách đang lặp
        var charactersToDestroy = new List<Character>(Characters);
        foreach (var character in charactersToDestroy)
        {
            // Thêm kiểm tra null trước khi gọi DestroyCharacter
            if (character != null && character.gameObject != null)
            {
                character.DestroyCharacter();
            }
            else
            {
                Debug.LogWarning("Found a null or already destroyed character reference in the Characters list during destruction.");
            }
        }

        // Xóa danh sách nhân vật sau khi đã hủy
        Characters.Clear();
        Players.Clear();
        Enemies.Clear();
    }

    // Thêm phương thức OnDisable để cleanup khi scene bị unload
    private void OnDisable()
    {
        // Cleanup các shadow objects có thể còn lại
        var existingCanSats = FindObjectsOfType<CanSat>();
        foreach (var canSat in existingCanSats)
        {
            if (canSat.dancer != null)
            {
                canSat.dancer.DestroyCharacter();
                canSat.dancer = null;
            }

            if (canSat.assassin != null) 
            {
                canSat.assassin.DestroyCharacter();
                canSat.assassin = null;
            }
        }
    }

    // Thêm phương thức OnApplicationQuit để cleanup khi thoát game
    private void OnApplicationQuit()
    {
        // Tương tự như OnDisable để đảm bảo cleanup
        var existingCanSats = FindObjectsOfType<CanSat>();
        foreach (var canSat in existingCanSats)
        {
            if (canSat.dancer != null)
            {
                canSat.dancer.DestroyCharacter();
                canSat.dancer = null;
            }

            if (canSat.assassin != null)
            {
                canSat.assassin.DestroyCharacter();
                canSat.assassin = null;
            }
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
            Skills = SelectedCharacter.skillConfig.SkillConfigs,
            skillTurnType = GetSkillTurnType(SelectedCharacter),
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
            Skills = character.skillConfig.SkillConfigs,
            skillTurnType = GetSkillTurnType(character),
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
        if (character1.IsMainCharacter)
        {
            SetMainCell(character1.Info.Cell);
        }
        else if (character2.IsMainCharacter)
        {
            SetMainCell(character2.Info.Cell);
        }
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
        if (!_process) StartCoroutine(WaitForCharactersExitCamera(Players, ProceedToNextLevel));
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
        // Lọc bỏ nhân vật null trước khi bắt đầu đợi
        characters = characters.Where(c => c != null).ToList();
        
        if (characters.Count == 0)
        {
            Debug.LogWarning("No valid characters to wait for! Proceeding immediately.");
            CleanupSequence();
            action?.Invoke();
            yield break;
        }
        
        // Thêm hệ thống timeout - giới hạn thời gian đợi tối đa là 10 giây
        float timeoutDuration = 10f;
        float elapsedTime = 0f;
        
        // Chỉ đợi nhân vật ra khỏi camera hoặc hết timeout
        while (!AreAllCharactersOutOfCamera(characters, GetMainCamera()) && elapsedTime < timeoutDuration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Nếu hết thời gian đợi, ghi log để debug
        if (elapsedTime >= timeoutDuration)
        {
            Debug.LogWarning("Timeout reached waiting for characters to exit camera. Proceeding anyway.");
        }
        
        CleanupSequence();
        action?.Invoke();
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
        bool allOutOfCamera = true;
        
        foreach (var character in characters)
        {
            if (character == null) continue; // Bỏ qua nhân vật null
            
            if (IsCharacterVisible(character.transform.position, camera))
            {
                allOutOfCamera = false;
                break;
            }
        }
        
        return allOutOfCamera;
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

    private bool _process = false;

    public void ProceedToNextLevel()
    {
        if (_process) return;
        if (levelConfig.levelType == LevelType.Tutorial)
            NextLevel();
        else if (levelConfig.levelType == LevelType.Level1)
        {
            ShowAfterCredit();
        }

        _process = true;
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
        CanShowSkipTutorial = IsTutorialLevel;
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
        
        // Xử lý các character thông thường
        var chars = new List<Character>();
        for (int i = CurrentPlayerIndex; i < Characters.Count; i++)
        {
            chars.Add(Characters[i]);
        }

        for (int i = 0; i < CurrentPlayerIndex; i++)
        {
            chars.Add(Characters[i]);
        }

        // Tìm và lưu tất cả các bóng của Càn Sát
        var shadows = new List<(Shadow shadow, CanSat owner)>();
        foreach (var character in Characters)
        {
            if (character is CanSat canSat)
            {
                if (canSat.dancer != null)
                    shadows.Add((canSat.dancer, canSat));
                if (canSat.assassin != null)
                    shadows.Add((canSat.assassin, canSat));
            }
        }

        // Lưu các character thường
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
                actionPoints = character.Info.ActionPoints,
                isShadow = false,
                moveAmount = character.Info.MoveAmount,
                increasedDamageTaken = character.Info.IncreasedDamageTaken,
                maxMoveRange = character.Info.Attributes.maxMoveRange
            };
            
            // Thêm xử lý đặc biệt cho Hoắc Liên Hương
            if (character is HoacLienHuong hlh && hlh.CurrentShield != null)
            {
                // Lưu vị trí cell shield
                characterData.shieldCellPosition = hlh.CurrentShield.CellPosition;
            }
            
            // Lưu giá trị VenomousParasite nếu là Đoàn Gia Linh
            if (character.characterType == CharacterType.DoanGiaLinh)
            {
                var doanGiaLinh = character as DoanGiaLinh;
                if (doanGiaLinh != null)
                {
                    characterData.venomousParasite = doanGiaLinh.GetVenomousParasite();
                    //AlkawaDebug.Log(ELogCategory.SAVE, $"[SaveGame] Lưu Độc Trùng của Đoàn Gia Linh: {characterData.venomousParasite}");
                }
            }
            
            levelData.characterDatas.Add(characterData);
        }

        // Lưu các bóng
        foreach (var (shadow, owner) in shadows)
        {
            CharacterData shadowData = new CharacterData
            {
                characterType = shadow.characterType,
                points = shadow.Info.Cell.CellPosition,
                currentHp = shadow.Info.CurrentHp,
                currentMp = shadow.Info.CurrentMp,
                iD = shadow.CharacterId,
                effectInfo = GetEffects(shadow),
                actionPoints = shadow.Info.ActionPoints,
                isShadow = true,
                ownerID = owner.CharacterId,
                shadowType = shadow.characterType,
                moveAmount = shadow.Info.MoveAmount,
                increasedDamageTaken = shadow.Info.IncreasedDamageTaken,
                maxMoveRange = shadow.Info.Attributes.maxMoveRange
            };
            levelData.characterDatas.Add(shadowData);
        }

        levelData.SaveTime = DateTime.Now;
        levelData.levelType = levelConfig.levelType;
        levelData.currentRound = CurrentRound;
        
        SaveLoadManager.Instance.OnSave(SaveLoadManager.Instance.levels.Count, levelData);
        CoroutineDispatcher.Invoke(ShowNotification, 0.5f);

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
                else if (item is PoisonousBloodPoolEffect poisonPool)
                {
                    // Lưu vị trí của các cell bị ảnh hưởng
                    poisonPool.impactPositions.Clear();
                    foreach (var cell in poisonPool.impacts)
                    {
                        poisonPool.impactPositions.Add(cell.CellPosition);
                    }
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