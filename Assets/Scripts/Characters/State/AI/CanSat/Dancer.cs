public class Dancer : Shadow
{
    public override void OnDie()
    {
        owner.Info.RemoveAllEffect(EffectType.IncreaseDef);
        owner.Info.RemoveAllEffect(EffectType.IncreaseSpd);
        base.OnDie();
    }
}