using System;

[Serializable]
public class ChangeStatEffect : EffectData
{
    public int value;
}

[Serializable]
public class VenomousParasiteEffect : ChangeStatEffect
{
    // Lưu trữ số lượng hoa khi độc trùng được áp dụng
    public int associatedFlowers;
}
