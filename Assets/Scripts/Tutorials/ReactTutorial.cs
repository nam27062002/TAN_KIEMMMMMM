public class ReactTutorial : TutorialSequence
{
    
    protected override void OnEnable()
    {
        base.OnEnable();
        ReactMenu.Instance.OnOpen = OnOpen;
    }
    
    public void HandleClick()
    {
        if (!GameplayManager.Instance.IsTutorialLevel) return;
        if (!CanClick()) return;
        // GameplayManager.Instance.characterManager.OnConFirmClick();
        Tutorial.OnTutorialClicked(index);
    }
    
    public override void PrepareTutorial()
    {
       Tutorial.arrow.gameObject.SetActive(false);
    }

    public void OnOpen()
    {
        // if (Tutorial != null && GameplayManager.Instance.IsTutorialLevel)
        // {
        //     Tutorial.arrow.gameObject.SetActive(true);
        // }
    }
}