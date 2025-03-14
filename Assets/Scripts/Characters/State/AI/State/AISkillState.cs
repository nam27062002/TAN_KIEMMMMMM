public class AISkillState : SkillState
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
        if (!WaitForReact && GpManager.SelectedCharacter != null)
        {
            if (Character == GpManager.MainCharacter)
            {
                GameplayManager.Instance.HandleEndTurn("Đã dùng xong skill");   
            }
            else
            {
                GpManager.SetSelectedCharacter(GpManager.MainCharacter);
                GpManager.SetInteract(true);
            }
        }
    }
}