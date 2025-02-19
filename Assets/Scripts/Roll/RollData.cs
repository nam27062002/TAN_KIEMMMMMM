using System;

[Serializable]
public class RollData
{
    public int rollTime;
    public int rollValue;
    public int add;

    public RollData()
    {
        
    }

    public RollData(int rollTime, int rollValue, int add)
    {
        this.rollTime = rollTime;
        this.rollValue = rollValue;
        this.add = add;
    }
}