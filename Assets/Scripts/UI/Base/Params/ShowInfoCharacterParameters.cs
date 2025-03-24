using System;
using System.Collections.Generic;
using NUnit.Framework;

public class ShowInfoCharacterParameters : EventArgs, UIBaseParameters
{
    public Character Character;
    public Dictionary<SkillTurnType, List<SkillInfo>> Skills = new();
    public SkillTurnType skillTurnType;
}