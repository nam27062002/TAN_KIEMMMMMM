﻿public class AISkillState : SkillState
{
    public AISkillState(Character character) : base(character)
    {
    }
    
    protected override void HandleAllTargetFinish()
    {
        base.HandleAllTargetFinish();
        HandleEndTurn();
    }
    
    private void HandleEndTurn()
    {
        if (!WaitForReact && GpManager.SelectedCharacter != null ) GameplayManager.Instance.HandleEndTurn();
    }
}