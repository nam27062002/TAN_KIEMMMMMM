public class Dancer : Shadow
{
    public override void OnDie()
    {
        owner.Info.RemoveEffect(EffectType.IncreaseDef, this);
        owner.Info.RemoveEffect(EffectType.IncreaseSpd, this);
        base.OnDie();
    }
}