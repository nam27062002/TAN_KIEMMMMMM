public class ReactTutorial : TutorialSequence
{
    
    protected override void OnEnable()
    {
        base.OnEnable();
        ReactMenu.Instance.OnConFirmClick = HandleClick;
        ReactMenu.Instance.OnOpen = OnOpen;
    }
    
    private void HandleClick()
    {
        if (!CanClick()) return;
        Tutorial.OnTutorialClicked(index);
    }
    
    public override void PrepareTutorial()
    {
       Tutorial.arrow.gameObject.SetActive(false);
    }

    public void OnOpen()
    {
        if (Tutorial != null && GameplayManager.Instance.IsTutorialLevel)
        {
            Tutorial.arrow.gameObject.SetActive(true);
        }
    }
}