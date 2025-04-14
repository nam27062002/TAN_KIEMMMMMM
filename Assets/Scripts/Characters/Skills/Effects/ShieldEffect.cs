using System;
[Serializable]
public class ShieldEffect : ChangeStatEffect
{ 
    public int damage;
    public int LinkedCharacterId;
    public Character OtherCharacter;
}