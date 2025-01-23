public class ActionPointTutorial : TutorialSequence
{
    public void Awake()
    {
        highlightable.Unhighlight();
    }
    
    public override void PrepareTutorial()
    {
        highlightable.Highlight();
        Invoke(nameof(NextTutorial), 3f);
    }

    private void NextTutorial()
    {
        highlightable.Unhighlight();
        Tutorial.OnTutorialClicked(index);
    }
}