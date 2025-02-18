using System.Collections.Generic;

public class SkillStateParams : StateParams
{
    public Character Source;
    public IdleStateParams IdleStateParams;
    public SkillTurnType SkillTurnType;
    public SkillInfo SkillInfo;
    public List<Character> Targets;
    public bool EndTurnAfterFinish;
    public Cell TargetCell;
}