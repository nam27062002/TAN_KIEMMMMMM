using System.Collections.Generic;
using UnityEngine;

public class FlyingTempest : PassiveSkill
{
    [SerializeField] private int triggerCondition = 5;
    [SerializeField] private SkillInfo info;
    private int _currentMove;

    public override void RegisterEvents()
    {
        base.RegisterEvents();
        character.CharacterInfo.OnMoveAmount += OnMoveAmount;
    }

    public override void UnregisterEvents()
    {
        base.UnregisterEvents();
        character.CharacterInfo.OnMoveAmount -= OnMoveAmount;
    }

    private void OnMoveAmount(object sender, int moveAmount)
    {
        _currentMove = moveAmount;

        if (_currentMove < triggerCondition) return;
        character.PendingPassiveSkillsTrigger.Add(this);
        _currentMove = 0;
    }

    public override void OnTrigger()
    {
        base.OnTrigger();
        character.ChangeState(ECharacterState.Skill, new SkillStateParams
        {
            SkillTurnType = SkillTurnType.MyTurn,
            SkillInfo = info,
            Targets = new List<Character>(
                GameplayManager.Instance.MapManager.GetCharactersInRange(character.CharacterInfo.Cell, info)),
        });
    }
}