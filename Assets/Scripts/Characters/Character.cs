using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    //new 
    public int CharacterId { get; set; }
    public event EventHandler<Character> OnDeath;
    // old
    [Title("Character Type")] public CharacterType characterType;

    [Title("Animation"), Space]
    [SerializeField]
    private CharacterAnimationData characterAnimationData;

    [Title("References")]
    public HpBar hpBar;
    public UIFeedback uiFeedback;
    public LinkCharacter linkCharacter;
    public GameObject model;

    [Title("Settings"), Space(10)] public CharacterConfig characterConfig;
    public SkillConfig skillConfig;
    public List<PassiveSkill> passiveSkills;

    public CharacterStateMachine StateMachine { get; set; }

    public HashSet<PassiveSkill> PendingPassiveSkillsTrigger { get; set; } = new();

    public CharacterInfo Info;
    protected SkillStateParams SkillStateParams;
    protected DamageTakenParams DamageTakenParams;
    public bool IsMainCharacter => GpManager.MainCharacter == this;
    // protected function
    protected GameplayManager GpManager => GameplayManager.Instance;
    protected MapManager MapManager => GpManager.MapManager;

    // Public function
    public bool lastDamageTakenCountered;
    public CharacterAnimationData AnimationData => characterAnimationData;

    // public vitual function
    public virtual Type Type => Type.None;
    public virtual bool CanEndTurn => false;
    public bool IsCounter => GetIdleStateParams() != null;

    public bool CanUseSkill { get; set; } = true;
    // Info
    public int GetMaxHp()
    {
#if UNITY_EDITOR
        return Info.Attributes.overrideMaxHp ? Info.Attributes.maxHpOverride : Info.Attributes.health;
#else
        return Info.Attributes.health;
#endif
    }

    private void Awake()
    {
        SetStateMachine();
        GpManager.OnEndTurn += OnEndTurn;
        GpManager.OnNewRound += OnNewRound;
    }

    private void OnEndTurn(object sender, EventArgs e)
    {
        if (SkillStateParams is { IdleStateParams: { DamageTakenParams: not null } })
        {
            SkillStateParams.IdleStateParams.DamageTakenParams.OnSetDamageTakenFinished -= OnDamageTakenCounterFinished;
        }
        CanUseSkill = true;
        SkillStateParams = null;
    }

    public void FixedUpdate()
    {
        DrawLink();
    }

    protected abstract void SetStateMachine();

    public virtual void Initialize(Cell cell, int iD)
    {
        CharacterId = iD;
        Info = new CharacterInfo(skillConfig, characterConfig.characterAttributes, this);
        skillConfig.SetSkillConfigs();
        SetCell(cell);
        SetIdle();
        SetSpeed();
        Info.OnHpChanged += OnHpChanged;
        Info.OnShieldChanged += OnShieldChanged;
        OnHpChanged(null);
        OnShieldChanged(null, 0);
        ChangeState(ECharacterState.Idle);
        SetPassiveSkills();
        StartCoroutine(HandleSpecialAction());
        AlkawaDebug.Log(ELogCategory.EDITOR, $"Initialize: {characterConfig.characterName} - {iD}");
    }

    public virtual void SetMainCharacter()
    {
        Info.ResetBuffBefore();
    }

    protected virtual IEnumerator HandleSpecialAction()
    {
        yield return null;
    }

    private void SetPassiveSkills()
    {
        foreach (var item in passiveSkills)
        {
            item.RegisterEvents();
        }
    }

    #region Set States

    protected virtual void SetIdle(IdleStateParams idleStateParams = null)
    {
        ChangeState(ECharacterState.Idle, idleStateParams);
    }

    private void SetSkill(SkillStateParams skillStateParams)
    {
        ChangeState(ECharacterState.Skill, skillStateParams);
    }

    public virtual void TryMoveToCell(Cell cell)
    {
        if (Info.MoveRange != null && Info.MoveRange.Contains(cell))
        {
            TryMoveToCell(MapManager.FindPath(Info.Cell, cell));
        }
    }

    public Cell GetBackCell()
    {
        Cell result = null;
        var cells = MapManager.GetHexagonsInMoveRange(Info.Cell, 1, DirectionType.All).ToList();
        var facing = model.transform.localScale.x == 1f ? FacingType.Right : FacingType.Left;
        if (facing == FacingType.Right)
        {
            result = cells.FirstOrDefault(p => p.CellPosition == new Vector2Int(Info.Cell.CellPosition.x, Info.Cell.CellPosition.y - 1));
        }
        else if (facing == FacingType.Left)
        {
            result = cells.FirstOrDefault(p => p.CellPosition == new Vector2Int(Info.Cell.CellPosition.x, Info.Cell.CellPosition.y + 1));
        }

        if (result == null)
        {
            result = cells[0];
        }

        return result;
    }

    protected void TryMoveToCell(List<Cell> cells)
    {
        ChangeState(ECharacterState.Move, new MoveStateParams(cells));
        Info.TryApplyBleedEffectWithMove(cells.Count);
    }

    public void ChangeState(ECharacterState newState, StateParams stateParams = null)
    {
        StateMachine.ChangeState(newState, stateParams);
    }

    public void HandleCounterLogic(SkillStateParams skillStateParams)
    {
        SkillStateParams = skillStateParams;
        HandleCounterLogic(skillStateParams.IdleStateParams.DamageTakenParams, true);
    }

    private void HandleCounterLogic(DamageTakenParams damageTaken = null, bool waitCounter = false)
    {
        SetIdle();
        DamageTakenParams = damageTaken ?? GetIdleStateParams().DamageTakenParams;
        DamageTakenParams.CanCounter = false;
        DamageTakenParams.WaitCounter = waitCounter;

        if (DamageTakenParams.WaitCounter)
        {
            DamageTakenParams.OnSetDamageTakenFinished += OnDamageTakenCounterFinished;
        }

        if (!HandleBlockSkillLogic(DamageTakenParams))
        {
            OnDamageTaken(DamageTakenParams);
        }
    }

    private bool HandleBlockSkillLogic(DamageTakenParams damageTakenParams)
    {
        if (SkillStateParams != null && CanBlockSkill(damageTakenParams))
        {
            damageTakenParams.OnSetDamageTakenFinished?.Invoke(new FinishApplySkillParams
            {
                Character = this,
                WaitForCounter = true,
            });
            ShowMessage("Chặn sát thương");
            return true;
        }
        return false;
    }

    protected virtual bool CanBlockSkill(DamageTakenParams damageTakenParams)
    {
        return SkillStateParams.SkillInfo.canBlockDamage;
    }

    private void OnDamageTakenCounterFinished(FinishApplySkillParams _)
    {
        if (Info.IsDie) return;
        SkillStateParams.IdleStateParams.DamageTakenParams.OnSetDamageTakenFinished -= OnDamageTakenCounterFinished;
        CoroutineDispatcher.Invoke(HandleCounter, 1f);
    }

    private void HandleCounter()
    {
        SkillStateParams.IdleStateParams = null;
        SkillStateParams.EndTurnAfterFinish = true;
        CanUseSkill = false;
        SkillStateParams.DamageTakenParams = DamageTakenParams;
        SetSkill(SkillStateParams);
    }

    #endregion

    #region Skills

    public bool TryCastSkill(Cell cell)
    {
        if (cell == null)
        {
            Debug.LogError("TryCastSkill: cell is null");
            return false;
        }

        if (Info.SkillInfo == null) return false;
        var damageType = Info.SkillInfo.damageType;
        if (GpManager.SelectedCharacter.Type == Type.Player && GpManager.MainCharacter.Type == Type.AI && Info.SkillInfo.isDirectionalSkill && !Info.SkillInfo.damageType.HasFlag(DamageTargetType.Move))
        {
            if (cell.Character != GpManager.MainCharacter) return false;
            HandleCastSkill(new List<Character>() { cell.Character });
            return true;
        }

        if (damageType.HasFlag(DamageTargetType.Enemies) || damageType.HasFlag(DamageTargetType.Team))
        {
            if (!Info.SkillRange.Contains(cell)) return false;
            HandleCastSkill(new List<Character>() { cell.Character });
            return true;
        }

        if (damageType.HasFlag(DamageTargetType.Move))
        {
            if (!Info.SkillRange.Contains(cell)) return false;
            HandleCastSkill(null, cell);
            return true;
        }
        return false;

    }

    public List<SkillInfo> GetSkillInfos(SkillTurnType skillTurnType)
    {
        return skillConfig.SkillConfigs[skillTurnType];
    }

    private void HandleCastSkill(List<Character> targets = null, Cell targetCell = null, SkillTurnType skillTurnType = SkillTurnType.None, bool dontNeedActionPoints = false)
    {
        HandleMpChanged(-Info.SkillInfo.mpCost);

        var skillParams = new SkillStateParams
        {
            IdleStateParams = GetIdleStateParams(),
            SkillInfo = Info.SkillInfo,
            Targets = targets ?? new List<Character>(),
            SkillTurnType = skillTurnType == SkillTurnType.None ? GetSkillTurnType() : skillTurnType,
            TargetCell = targetCell,
            Source = this,
        };

        SetSkill(skillParams);
        if (!dontNeedActionPoints) Info.ReduceActionPoints();
        UnSelectSkill();
    }

    public void HandleCastSkill(List<Character> targets, SkillInfo skillInfo)
    {
        Info.SkillInfo = skillInfo;
        HandleCastSkill(targets);
    }

    public virtual void HandleMpChanged(int value)
    {
        if (value == 0) return;
        var dragon = Info.DragonArmorEffectData;
        if (dragon != null)
        {
            if (dragon.Actor != null)
            {
                value = Utils.RoundNumber(value * 1f / 2f);
                dragon.Actor.HandleMpChanged(value);
            }
            else
            {
                Debug.LogError("Loi roi");
            }
        }

        Info.CurrentMp += value;
        if (Info.CurrentMp < 0)
        {
            Info.CurrentMp = 0;
        }
        Info.OnMpChangedInvoke(value);
    }

    protected void HandleCastSkill(SkillInfo skillInfo, List<Character> targets = null, SkillTurnType skillTurnType = SkillTurnType.None, bool dontNeedActionPoints = false)
    {
        Info.SkillInfo = skillInfo;
        HandleCastSkill(targets, null, skillTurnType, dontNeedActionPoints: dontNeedActionPoints);
    }

    public HashSet<Cell> NonDirectionalCells = new HashSet<Cell>();

    public void HandleSelectSkill(int skillIndex, Skill_UI skillUI)
    {
        HideMoveRange();
        UnSelectSkill();
        
        // Kiểm tra tính hợp lệ của skillIndex trước khi gọi GetSkillInfo
        SkillInfo selectedSkill = GetSkillInfo(skillIndex);
        if (selectedSkill == null)
        {
            Debug.LogWarning($"[{characterConfig.characterName}] HandleSelectSkill: Không tìm thấy skill với index {skillIndex}");
            return;
        }
        
        if (Info.SkillInfo == selectedSkill) return;
        UnSelectSkill();
        Info.SkillInfo = selectedSkill;
        
        if (Info.SkillInfo.isDirectionalSkill)
        {
            HandleDirectionalSkill();
        }
        else
        {
            if (Info.Cell == null)
            {
                Debug.LogWarning($"[{characterConfig.characterName}] HandleSelectSkill: Cell của nhân vật là null!");
                return;
            }
            
            NonDirectionalCells = MapManager.GetAllHexagonInRange(Info.Cell, Info.SkillInfo.range);
            if (NonDirectionalCells != null)
            {
                foreach (var cell in NonDirectionalCells)
                {
                    cell.ShowSkillRange();
                }
            }
            HandleCastSkill();
            if (skillUI != null)
            {
                skillUI.highlightable.Unhighlight();
            }
        }
    }

    public void ShowSkillTarget(SkillInfo skillInfo)
    {
        HideMoveRange();
        NonDirectionalCells = MapManager.GetAllHexagonInRange(Info.Cell, skillInfo.range);
        foreach (var cell in NonDirectionalCells)
        {
            cell.ShowSkillRange();
        }
    }

    public void HideSkillTarget()
    {
        foreach (var cell in NonDirectionalCells)
        {
            cell.HideSkillRange();
        }
        NonDirectionalCells.Clear();
    }

    private void HandleDirectionalSkill()
    {
        ShowSkillRange();
    }

    protected IdleStateParams GetIdleStateParams()
    {
        if (StateMachine.CurrentState is IdleState idleState)
        {
            return idleState.IdleStateParams;
        }
        return null;
    }

    public SkillTurnType GetSkillTurnType()
    {
        return GpManager.GetSkillTurnType(this);
    }

    private SkillInfo GetSkillInfo(int index)
    {
        return Info.GetSkillInfo(index, GetSkillTurnType());
    }

    private void UnSelectSkill()
    {
        Info.SkillInfo = null;
        Skill_UI.Selected?.highlightable.Unhighlight();
        HideSkillRange();
    }

    public void HandleEndReact()
    {
        HandleCounterLogic();
    }

    public virtual void OnDamageTaken(DamageTakenParams damageTakenParams)
    {
        ChangeState(ECharacterState.DamageTaken, damageTakenParams);
    }

    #endregion

    #region Sub

    public void SetSelectedCharacter(IdleStateParams idleStateParams = null)
    {
        OnSelected();
        SetIdle(idleStateParams);
    }

    public void SetCell(Cell cell)
    {
        Info.Cell = cell;
        cell.OnCharacterRegister(this);
    }

    protected virtual void SetSpeed()
    {
        Info.SetSpeed();
    }

    public void OnHpChanged(object sender, int value = 0)
    {
        var currentHp = Info.CurrentHp;
        var maxHp = GetMaxHp();
        hpBar.SetValue(currentHp * 1f / maxHp, $"{currentHp} / {maxHp}");
    }

    private void OnShieldChanged(object sender, float value)
    {
        hpBar.SetShield(value);
    }

    public void ShowHpBar()
    {
        hpBar.gameObject.SetActiveIfNeeded(true);
    }

    public void HideHpBar()
    {
        hpBar.gameObject.SetActiveIfNeeded(false);
    }

    protected virtual void OnSelected()
    {
        Info.Cell.ShowFocus();
    }

    public virtual void OnUnSelected()
    {
        Info.Cell.HideFocus();
        UnSelectSkill();
        HideMoveRange();
    }

    public void ShowMoveRange()
    {
        if (!IsMainCharacter) return;
        Info.MoveRange = MapManager.GetHexagonsInMoveRange(Info.Cell, Info.GetMoveRange(), characterConfig.moveDirection);
        foreach (var item in Info.MoveRange)
        {
            item.ShowMoveRange();
        }
        UnSelectSkill();
    }

    public void HideMoveRange()
    {
        if (Info.MoveRange == null || Info.MoveRange.Count == 0) return;
        foreach (var item in Info.MoveRange)
        {
            item.HideMoveRange();
        }
        Info.MoveRange.Clear();
    }

    private void ShowSkillRange()
    {
        if (Info == null || Info.Cell == null || Info.SkillInfo == null)
        {
            Debug.LogWarning($"[{characterConfig.characterName}] ShowSkillRange: Info, Cell hoặc SkillInfo là null!");
            return;
        }
        
        Info.SkillRange = MapManager.GetHexagonsInAttack(Info.Cell, Info.SkillInfo);
        if (Info.SkillRange == null)
        {
            Debug.LogWarning($"[{characterConfig.characterName}] ShowSkillRange: Không thể lấy được SkillRange!");
            return;
        }
        
        foreach (var item in Info.SkillRange)
        {
            item.ShowSkillRange();
        }

        if (Info.SkillRange.Count == 0)
        {
            UIManager.Instance.ShowNotification("Không có mục tiêu nào trong tầm", 2f);
        }
    }

    private void HideSkillRange()
    {
        if (Info.SkillRange == null || Info.SkillRange.Count == 0) return;
        foreach (var item in Info.SkillRange)
        {
            item.HideSkillRange();
        }
        Info.SkillRange.Clear();
    }

    public void ShowMessage(string message, bool isCrit = false)
    {
        uiFeedback.ShowMessage(message, isCrit);
    }

    public void UnRegisterCell()
    {
        if (Info != null && Info.Cell != null && Info.Cell.CellType == CellType.Character)
        {
            Info.Cell.CellType = CellType.Walkable;
            Info.Cell.Character = null;
            Info.Cell.HideFocus();
        }

    }
    #endregion

    private void DrawLink()
    {
        if (this == null || Info.IsDie) return;
        if (Info == null) return;
        var coverEffect = Info.CoverEffectData;
        if (coverEffect == null) return;
        linkCharacter.ClearLine();
        linkCharacter.SetLine(coverEffect.Actor.transform.position, transform.position);
    }

    public virtual void DestroyCharacter()
    {
        if (gameObject == null)
        {
            Debug.LogWarning($"[{characterConfig?.characterName ?? "Unknown Character"}] Attempted to destroy an already destroyed character or null gameObject.");
            return;
        }

        if (gameObject != null && Info != null)
        {
            UnRegisterCell();
        }

        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    public virtual void HandleDeath()
    {
        Debug.Log($"HandleDeath: {characterConfig.characterName}");
        GpManager.HandleCharacterDeath(this, out var callback);
        OnDeath?.Invoke(this, this);
        GpManager.OnNewRound -= OnNewRound;
        callback?.Invoke();
        UnRegisterCell();
        Destroy(gameObject);
    }

    protected virtual void OnNewRound(object sender, EventArgs e)
    {

    }

    public void SetPosition()
    {
        StateMachine.GetCurrentState.SetCharacterPosition();
        UpdateFacing();
    }

    public void UpdateFacing()
    {
        StateMachine.GetCurrentState.SetFacing();
    }

    public void SetFacing(FacingType facingType)
    {
        StateMachine.GetCurrentState.SetFacing(facingType);
    }

    public void TeleportToCell(Cell cell)
    {
        Info.Cell.HideFocus();
        Info.Cell.Character = null;
        Info.Cell.CellType = CellType.Walkable;

        cell.Character = this;
        cell.CellType = CellType.Character;
        Info.Cell = cell;
        var pos = Info.Cell.transform.position;
        pos.y += characterConfig.characterHeight / 2f;
        pos.z = pos.y;
        transform.position = pos;
        UpdateFacing();

        if (IsMainCharacter)
        {
            GpManager.SetMainCell(cell);
        }
    }

    public virtual int GetSkillActionPoints(SkillTurnType skillTurnType) => characterConfig.actionPoints[skillTurnType];



    private List<SkillInfo> GetValidSkills()
    {
        var skillType = GpManager.GetSkillTurnType(this);
        return GetSkillInfos(skillType)
            .Where(skill => Info.CanCastSkill(skill))
            .ToList();
    }

    public List<CastSkillData> GetValidSkills(Character character)
    {
        if (character == null)
        {
            Debug.LogException(new ArgumentNullException(nameof(character)));
            return null;
        }
        var validSkills = GetValidSkills();
        if (validSkills == null) return null;
        List<CastSkillData> skills = new List<CastSkillData>();
        if (character.Type == Type) // Đồng minh
        {
            foreach (var skill in validSkills)
            {
                if (skill.damageType.HasFlag(DamageTargetType.Team))
                {
                    var teammatesInRange = GpManager.GetTeammatesInRange(this, skill.range, skill.directionType);
                    if (teammatesInRange.Contains(character))
                    {
                        skills.Add(new CastSkillData()
                        {
                            SkillInfo = skill,
                            CharactersImpact = new List<Character>() { character },
                            MaxCharactersImpact = 1,
                        });
                    }
                }
            }
        }
        else // Đối thủ
        {
            foreach (var skill in validSkills)
            {
                if (skill.damageType.HasFlag(DamageTargetType.Enemies))
                {
                    var enemiesInRange = GpManager.GetEnemiesInRange(this, skill.range, skill.directionType);
                    if (enemiesInRange.Contains(character))
                    {
                        skills.Add(new CastSkillData()
                        {
                            SkillInfo = skill,
                            CharactersImpact = new List<Character>() { character },
                            MaxCharactersImpact = 1
                        });
                    }
                }
            }
        }

        return skills;
    }


#if UNITY_EDITOR

    [Button("Reduce Hp Editor")]
    public void ReduceHpEditor()
    {
        Info.CurrentHp -= 2;
        Info.OnHpChangedInvoke(-2);
        Info.OnHpChangedInvoke(-2);
    }

    private void OnValidate()
    {
        if (characterAnimationData == null)
        {
            characterAnimationData = GetComponentInChildren<CharacterAnimationData>();
        }

        if (model == null)
        {
            model = gameObject.FindChildByName("Model");
        }

        if (hpBar == null)
        {
            hpBar = GetComponentInChildren<HpBar>();
        }

        if (uiFeedback == null)
        {
            uiFeedback = GetComponentInChildren<UIFeedback>();
        }

        if (linkCharacter == null)
        {
            linkCharacter = GetComponentInChildren<LinkCharacter>();
        }
        passiveSkills = GetComponents<PassiveSkill>().ToList();

        skillConfig.OnValidate();
    }
#endif
}