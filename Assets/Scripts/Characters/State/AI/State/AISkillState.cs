public class AISkillState : SkillState
{
    public AISkillState(Character character) : base(character)
    {
    }
    
    protected override void HandleAllTargetFinish()
    {
        base.HandleAllTargetFinish();
        GameplayManager.Instance.HandleEndTurn();
    }
}