using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [Title("Character Type")] public CharacterType characterType;

    [Title("Animation"), Space] [SerializeField]
    private CharacterAnimationData characterAnimationData;

    [Title("References")] 
    public HpBar hpBar;
    public UIFeedback uiFeedback;
    public GameObject model;

    [Title("Settings"), Space(10)] public CharacterConfig characterConfig;
    public SkillConfig skillConfig;
    public List<PassiveSkill> passiveSkills;

    public CharacterStateMachine StateMachine { get; set; }

    public HashSet<PassiveSkill> PendingPassiveSkillsTrigger { get; set; } = new();
    
    public CharacterInfo Info;
    private SkillStateParams _skillStateParams;
    public bool IsMainCharacter => GpManager.MainCharacter == this;
    // protected function
    protected GameplayManager GpManager => GameplayManager.Instance;
    protected MapManager MapManager => GpManager.MapManager;
    
    // Public function
    public CharacterAnimationData AnimationData => characterAnimationData;
    
    // public vitual function
    public virtual Type Type => Type.None;
    public virtual bool CanEndTurn => false;
    public bool IsReact => GetIdleStateParams() != null;
    private void Awake()
    {
        SetStateMachine();
    }

    protected abstract void SetStateMachine();

    public void Initialize(Cell cell)
    {
        Info = new CharacterInfo(skillConfig, characterConfig.characterAttributes, this);
        skillConfig.SetSkillConfigs();
        SetCell(cell);
        SetIdle();
        SetSpeed();
        Info.OnHpChanged += OnHpChanged;
        OnHpChanged(null);
        ChangeState(ECharacterState.Idle);
        SetPassiveSkills();
    }

    public virtual void SetMainCharacter()
    {
        Info.ResetBuffBefore();
    }

    private void SetPassiveSkills()
    {
        foreach (var item in passiveSkills)
        {
            item.RegisterEvents();
        }
    }

    #region Set States
    
    private void SetIdle(IdleStateParams idleStateParams = null)
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

    protected virtual void TryMoveToCell(List<Cell> cells)
    {
        ChangeState(ECharacterState.Move, new MoveStateParams(cells));
    }

    public void ChangeState(ECharacterState newState, StateParams stateParams = null)
    {
        StateMachine.ChangeState(newState, stateParams);
    }

    public void HandleCounterLogic(SkillStateParams skillStateParams)
    {
        _skillStateParams = skillStateParams;
        HandleCounterLogic(skillStateParams.IdleStateParams.DamageTakenParams, true);
    }
    
    private void HandleCounterLogic(DamageTakenParams damageTaken = null, bool waitCounter = false) // 
    {
        SetIdle();
        var damageTakenParams = damageTaken ?? GetIdleStateParams().DamageTakenParams;
        damageTakenParams.CanCounter = false;
        damageTakenParams.WaitCounter = waitCounter;
        if (damageTakenParams.WaitCounter)
        {
            damageTakenParams.OnSetDamageTakenFinished += OnDamageTakenCounterFinished;
        }
        
        if (_skillStateParams != null && _skillStateParams.SkillInfo.canBlockDamage)
        {
            damageTakenParams.OnSetDamageTakenFinished?.Invoke(new FinishApplySkillParams
            {
                Character = this,
                WaitForCounter = true,
            });
            ShowMessage("Chặn sát thương");
        }
        else
        {
            OnDamageTaken(damageTakenParams); 
        }
        
    }

    private void OnDamageTakenCounterFinished(FinishApplySkillParams _)
    {
        _skillStateParams.IdleStateParams.DamageTakenParams.OnSetDamageTakenFinished -= OnDamageTakenCounterFinished;
        CoroutineDispatcher.Invoke(HandleCounter, 1f);
    }

    private void HandleCounter()
    {
        _skillStateParams.IdleStateParams = null;
        _skillStateParams.EndTurnAfterFinish = true;
        SetSkill(_skillStateParams);
    }
    
    #endregion

    #region Skills

    public bool TryCastSkill(Cell cell)
    {
        if (Info.SkillInfo == null) return false;
        var damageType = Info.SkillInfo.damageType;
        
        if (damageType.HasFlag(DamageTargetType.Enemies) || damageType.HasFlag(DamageTargetType.Team))
        {
            if (!Info.SkillRange.Contains(cell)) return false;
            HandleCastSkill(new List<Character>(){cell.Character});
            return true;
        }

        if (damageType.HasFlag(DamageTargetType.Move))
        {
            if (!Info.SkillRange.Contains(cell)) return false;
            var path = MapManager.FindShortestPath(Info.Cell, cell);
            var targets = (from item in path where item.CellType == CellType.Character && item.Character.Type == Type.AI select item.Character).ToList();
            HandleCastSkill(targets, cell);
            return true;
        }
        return false;
    }
    
    public List<SkillInfo> GetSkillInfos(SkillTurnType skillTurnType)
    {
        return skillConfig.SkillConfigs[skillTurnType];
    }

    private void HandleCastSkill(List<Character> targets = null, Cell targetCell = null)
    {
        Info.HandleMpChanged(-Info.SkillInfo.mpCost);
    
        var skillParams = new SkillStateParams
        {
            IdleStateParams = GetIdleStateParams(),
            SkillInfo = Info.SkillInfo,
            Targets = targets ?? new List<Character>(),
            SkillTurnType = GetSkillTurnType(),
            TargetCell = targetCell,
        };
    
        SetSkill(skillParams);
        Info.ReduceActionPoints();
        UnSelectSkill();
    }

    protected void HandleCastSkill(SkillInfo skillInfo, List<Character> targets = null)
    {
        Info.SkillInfo = skillInfo;
        HandleCastSkill(targets);
    }

    public void HandleSelectSkill(int skillIndex)
    {
        HideMoveRange();
        UnSelectSkill();
        if (Info.SkillInfo == GetSkillInfo(skillIndex)) return;
        UnSelectSkill();
        Info.SkillInfo = GetSkillInfo(skillIndex);
        if (Info.SkillInfo.isDirectionalSkill)
        {
            HandleDirectionalSkill();
        }
        else
        {
            HandleCastSkill();
        }
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
    
    public void OnDamageTaken(DamageTakenParams damageTakenParams)
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
    
    private void OnHpChanged(object sender, int value = 0)
    {
        var currentHp = Info.CurrentHp;
        var maxHp = Info.Attributes.health;
        hpBar.SetValue(currentHp * 1f / maxHp, $"{currentHp} / {maxHp}");
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
        Info.SkillRange = MapManager.GetHexagonsInAttack(Info.Cell, Info.SkillInfo);
        foreach (var item in Info.SkillRange)
        {
            item.ShowSkillRange();
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

    public void ShowMessage(string message)
    {
        uiFeedback.ShowMessage(message);
    }

    public void UnRegisterCell()
    {
        Info.Cell.CellType = CellType.Walkable;
        Info.Cell.Character = null;
        Info.Cell.HideFocus();
    }
    #endregion
    
    public void DestroyCharacter()
    {
        Destroy(gameObject);
        foreach (var item in passiveSkills)
        {
            item.UnregisterEvents();
        }
    }

    public void OnDie()
    {
        Info.Cell.CellType = CellType.Walkable;
        var index = GpManager.Characters.IndexOf(this);
        ((UI_Ingame)UIManager.Instance.CurrentMenu).OnCharacterDeath(index);
        GpManager.HandleCharacterDie(this);
        if (IsMainCharacter)
        {
            GpManager.HandleEndTurn();
        }
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(characterAnimationData == null)
        {
            characterAnimationData = GetComponentInChildren<CharacterAnimationData>();
        }

        if(model == null)
        {
            model = gameObject.FindChildByName("Model");
        }

        if(hpBar == null)
        {
            hpBar = GetComponentInChildren<HpBar>();
        }

        if(uiFeedback == null)
        {
            uiFeedback = GetComponentInChildren<UIFeedback>();
        }

        passiveSkills = GetComponents<PassiveSkill>().ToList();
        
        skillConfig.OnValidate();
    }
#endif
}