public class ReactTutorial : TutorialSequence
{
    public ReactPopup reactPopup;
    
    public void HandleClick()
    {
        if (!GameplayManager.Instance.IsTutorialLevel) return;
        if (!CanClick()) return;
        reactPopup.OnConfirm?.Invoke();
        Tutorial.OnTutorialClicked(index);
    }
    
    public override void PrepareTutorial()
    {
       Tutorial.arrow.gameObject.SetActive(false);
    }
}