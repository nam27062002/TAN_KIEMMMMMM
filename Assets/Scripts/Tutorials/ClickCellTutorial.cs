using UnityEngine;

public class ClickCellTutorial : TutorialSequence
{
    public Cell cell;
    public void OnMouseDown()
    {
        
    }
        
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (!CanClick()) return;
            Tutorial.arrow.gameObject.SetActive(false);
            GameplayManager.Instance.ShowInfo(cell.Character);
        }
        else if (Input.GetMouseButtonDown(0) && index != 13)
        {
            if (!CanClick()) return;
            OnFinishTutorial();
            cell.HandleCellClicked();
        }
    }
    
    public override void PrepareTutorial()
    {
    }
        
    public void OnValidate()
    {
        if (cell == null)
        {
            cell = GetComponent<Cell>();
        }
    }
}