using System.Collections.Generic;

public class MoveStateParams : StateParams
{
    public List<Cell> MoveCells;

    public MoveStateParams(List<Cell> moveCells)
    {
        MoveCells = moveCells;
    }
}