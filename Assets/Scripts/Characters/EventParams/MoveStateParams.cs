using System.Collections.Generic;

public class MoveStateParams : StateParams
{
    public readonly List<Cell> MoveCells;

    public MoveStateParams(List<Cell> moveCells)
    {
        MoveCells = moveCells;
    }
}