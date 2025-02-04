using System;
using System.Collections.Generic;

public class ShowInfoCharacterParameters : EventArgs, UIBaseParameters
{
    public Character Character;
    public List<SkillInfo> Skills;
}