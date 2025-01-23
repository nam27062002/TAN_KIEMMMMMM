using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ClickSkillTutorial : TutorialSequence
{
    public Button button;
    public int skillIndex;
    
    public override void Start()
    {
        base.Start();
        highlightable.Unhighlight();
        button.onClick.AddListener(HandleMouseDown);
    }
        
    private void HandleMouseDown()
    {
        if (!CanClick()) return;
        highlightable.Unhighlight();
        GameplayManager.Instance?.HandleSelectSkill(skillIndex);
        Tutorial.OnTutorialClicked(index);
    }

    public override void PrepareTutorial()
    {
        highlightable.Highlight();
    }
}