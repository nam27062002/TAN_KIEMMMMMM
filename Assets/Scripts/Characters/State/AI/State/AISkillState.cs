using System.Collections.Generic;
using UnityEngine;

public class AISkillState : SkillState
{
    public AISkillState(Character character) : base(character)
    {
    }
    
    protected override void HandleAllTargetFinish()
    {
        base.HandleAllTargetFinish();
        if (!WaitForReact)
        {
            GameplayManager.Instance.HandleEndTurn(1f, "Finished using skill");
        }
    }
    
    private void HandleEndTurn()
    {
        if (!WaitForReact && GpManager.SelectedCharacter != null)
        {
            if (Character == GpManager.MainCharacter)
            {
                GameplayManager.Instance.HandleEndTurn(1f, "Finished using skill");   
            }
            else
            {
                GpManager.SetSelectedCharacter(GpManager.PreviousSelectedCharacter != null ? GpManager.PreviousSelectedCharacter : GpManager.MainCharacter);
                GpManager.SetInteract(true);
            }
        }
    }
}