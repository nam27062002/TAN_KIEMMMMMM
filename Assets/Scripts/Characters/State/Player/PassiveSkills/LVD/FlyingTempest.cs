using UnityEngine;

public class FlyingTempest : PassiveSkill
{
    [SerializeField] private LyVoDanh lyVoDanh;
    [SerializeField] private int triggerCondition = 5;
    private int _currentMove;
    
    public override void RegisterEvents()
    {
        base.RegisterEvents();
        lyVoDanh.CharacterInfo.OnMoveAmount += OnMoveAmount;
    }
    
    public override void UnregisterEvents()
    {
        base.UnregisterEvents();
        lyVoDanh.CharacterInfo.OnMoveAmount -= OnMoveAmount;
    }
    
    private void OnMoveAmount(object sender, int moveAmount)
    {
        _currentMove = moveAmount;

        if (_currentMove >= triggerCondition)
        {
            // lyVoDanh.PendingPassiveSkillsTrigger.Add(this);
            _currentMove = 0;
        }
    }

    public override void OnTrigger()
    {
        base.OnTrigger();
        var targets = GameplayManager.Instance.MapManager.GetCharacterInRange(lyVoDanh.CharacterInfo.Cell, 1);
        // lyVoDanh.PlayAnim(AnimationParameterNameType.Skill1, OnEndAnim);
        // foreach (var target in targets)
        // {
        //     if (target.Type == Type.AI) target.CharacterInfo.OnDamageTaken(lyVoDanh.CharacterInfo.BaseDamage);
        // }
    }

    private void OnEndAnim()
    {
        lyVoDanh.ChangeState(ECharacterState.Idle);
    }

    private void OnValidate()
    {
        lyVoDanh ??= GetComponent<LyVoDanh>();
    }
}