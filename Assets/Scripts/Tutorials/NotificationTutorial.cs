public class NotificationTutorial : TutorialSequence
{
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