using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ClickSkillTutorial : TutorialSequence
{
    public Button button;
    public int skillIndex;
    public Skill_UI skillUI;
    public override void Start()
    {
        base.Start();
        skillUI = GetComponentInChildren<Skill_UI>();
        highlightable.Unhighlight();
        button.onClick.AddListener(HandleMouseDown);
    }
        
    private void HandleMouseDown()
    {
        if (!CanClick()) return;
        highlightable.Unhighlight();
        GameplayManager.Instance?.HandleSelectSkill(skillIndex, skillUI);
        Tutorial.OnTutorialClicked(index);
    }

    public override void PrepareTutorial()
    {
        highlightable.Highlight();
    }
}