public class ClickCellTutorial : TutorialSequence
{
    public Cell cell;
    public void OnMouseDown()
    {
        if (!CanClick()) return;
        OnFinishTutorial();
        cell.HandleCellClicked();
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