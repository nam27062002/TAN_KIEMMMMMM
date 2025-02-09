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

    protected CharacterStateMachine StateMachine { get; set; }

    public HashSet<PassiveSkill> PendingPassiveSkillsTrigger { get; set; } = new();
    
    public CharacterInfo CharacterInfo;
    
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
        CharacterInfo = new CharacterInfo(skillConfig, characterConfig.characterAttributes, this);
        skillConfig.SetSkillConfigs();
        SetCell(cell);
        SetIdle();
        SetSpeed();
        CharacterInfo.OnHpChanged += OnHpChanged;
        OnHpChanged(null);
        ChangeState(ECharacterState.Idle);
        SetPassiveSkills();
    }

    public virtual void SetMainCharacter()
    {
        CharacterInfo.HandleIncreaseValueActionPoints();
        CharacterInfo.ResetBuffBefore();
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
        if (CharacterInfo.MoveRange != null && CharacterInfo.MoveRange.Contains(cell))
        {
            TryMoveToCell(MapManager.FindPath(CharacterInfo.Cell, cell));
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
        SetIdle();
        OnDamageTaken(skillStateParams.IdleStateParams.DamageTakenParams);
    }

    private void HandleCounterLogic()
    {
        SetIdle();
        OnDamageTaken(GetIdleStateParams().DamageTakenParams);
    }
    
    #endregion

    #region Skills

    public bool TryCastSkill(Cell cell)
    {
        if (!CharacterInfo.CharactersInSkillRange.Contains(cell.Character)) return false;
        HandleCastSkill(new List<Character>(){cell.Character});
        return true;
    }
    
    public List<SkillInfo> GetSkillInfos(SkillTurnType skillTurnType)
    {
        return skillConfig.SkillConfigs[skillTurnType];
    }

    private void HandleCastSkill(List<Character> targets = null)
    {
        CharacterInfo.HandleMpChanged(-CharacterInfo.SkillInfo.mpCost);
    
        var skillParams = new SkillStateParams
        {
            IdleStateParams = GetIdleStateParams(),
            SkillInfo = CharacterInfo.SkillInfo,
            Targets = targets ?? new List<Character>(),
            SkillTurnType = GetSkillTurnType(),
        };
    
        SetSkill(skillParams);
        CharacterInfo.HandleReduceActionPoints();
        UnSelectSkill();
    }

    protected void HandleCastSkill(SkillInfo skillInfo, List<Character> targets = null)
    {
        CharacterInfo.SkillInfo = skillInfo;
        HandleCastSkill(targets);
    }

    public void HandleSelectSkill(int skillIndex)
    {
        HideMoveRange();
        UnSelectSkill();
        if (CharacterInfo.SkillInfo == GetSkillInfo(skillIndex)) return;
        UnSelectSkill();
        CharacterInfo.SkillInfo = GetSkillInfo(skillIndex);
        if (CharacterInfo.SkillInfo.isDirectionalSkill)
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
        CharacterInfo.CharactersInSkillRange.Clear();
        var damageType = CharacterInfo.SkillInfo.damageType;
        if (damageType.HasFlag(DamageTargetType.Self))
        {
            CharacterInfo.CharactersInSkillRange.Add(this);
        }
        
        foreach (var cell in CharacterInfo.SkillRange.Where(cell => cell.CellType == CellType.Character).Where(cell => IsValidTarget(cell.Character, damageType)))
        {
            CharacterInfo.CharactersInSkillRange.Add(cell.Character);
        }
        
        var counterCharacter = GetCounterCharacter();
        if (counterCharacter == null) return;
        var isValidCounter = CharacterInfo.CharactersInSkillRange.Contains(counterCharacter);
        CharacterInfo.CharactersInSkillRange.Clear();
        if (isValidCounter)
        {
            CharacterInfo.CharactersInSkillRange.Add(counterCharacter);
        }
    }
    
    private bool IsValidTarget(Character target, DamageTargetType damageType)
    {
        return damageType.HasFlag(target.Type == Type ? DamageTargetType.Team : DamageTargetType.Enemies);
    }
    
    private Character GetCounterCharacter()
    {
        if (StateMachine.CurrentState is IdleState { IdleStateParams: not null } idleState)
        {
            return idleState.IdleStateParams.DamageTakenParams.ReceiveFromCharacter;
        }
        return null;
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
        return CharacterInfo.GetSkillInfo(index, GetSkillTurnType());
    }
    
    private void UnSelectSkill()
    {
        CharacterInfo.SkillInfo = null;
        CharacterInfo.CharactersInSkillRange.Clear();
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
        CharacterInfo.Cell = cell;
        cell.OnCharacterRegister(this);
    }
    
    protected virtual void SetSpeed()
    {
        CharacterInfo.SetSpeed();
    }
    
    private void OnHpChanged(object sender, int value = 0)
    {
        var currentHp = CharacterInfo.CurrentHp;
        var maxHp = CharacterInfo.Attributes.health;
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
    
    public virtual void OnSelected()
    {
        CharacterInfo.Cell.ShowFocus();
    }

    public virtual void OnUnSelected()
    {
        CharacterInfo.Cell.HideFocus();
        UnSelectSkill();
        HideMoveRange();
    }

    public void ShowMoveRange()
    {
        if (!IsMainCharacter) return;
        CharacterInfo.MoveRange = MapManager.GetHexagonsInMoveRange(CharacterInfo.Cell, CharacterInfo.GetMoveRange());
        foreach (var item in CharacterInfo.MoveRange)
        {
            item.ShowMoveRange();
        }
        UnSelectSkill();
    }
    
    public void HideMoveRange()
    {
        if (CharacterInfo.MoveRange == null || CharacterInfo.MoveRange.Count == 0) return;
        foreach (var item in CharacterInfo.MoveRange)
        {
            item.HideMoveRange();
        }
        CharacterInfo.MoveRange.Clear();
    }
    
    private void ShowSkillRange()
    {
        CharacterInfo.SkillRange = MapManager.GetHexagonsInAttack(CharacterInfo.Cell, CharacterInfo.SkillInfo.range);
        foreach (var item in CharacterInfo.SkillRange)
        {
            item.ShowSkillRange();
        }
    }
    
    private void HideSkillRange()
    {
        if (CharacterInfo.SkillRange == null || CharacterInfo.SkillRange.Count == 0) return;
        foreach (var item in CharacterInfo.SkillRange)
        {
            item.HideSkillRange();
        }
        CharacterInfo.SkillRange.Clear();
    }

    public void ShowMessage(string message)
    {
        uiFeedback.ShowMessage(message);
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
        CharacterInfo.Cell.CellType = CellType.Walkable;
        var index = GpManager.Characters.IndexOf(this);
        ((UI_Ingame)UIManager.Instance.CurrentMenu).OnCharacterDeath(index);
        GpManager.HandleCharacterDie(this);
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