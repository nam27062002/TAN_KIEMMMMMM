using System;
using UnityEngine.UI;

public class EndTurnTutorial : TutorialSequence
{
    public Button button;
    public bool isWaiting;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        button.onClick.AddListener(OnMouseDown);
    }
    
    private void OnDisable()
    {
        button.onClick.RemoveListener(OnMouseDown);
    }
    
    public override void PrepareTutorial()
    {
        highlightable.Highlight();
    }
    
    public void OnMouseDown()
    {
        if (!CanClick()) return;
        highlightable.Unhighlight();
        if (!isWaiting)
        {
            Tutorial.OnTutorialClicked(index);
        }
        else
        {
            Tutorial.arrow.gameObject.SetActive(false);
            UIManager.Instance.TryClosePopup(PopupType.Message);
            Tutorial.OnTutorialClicked(index);
        }

        GameplayManager.Instance.HandleEndTurn("Clicked vào end turn");

    }
}