public class Dancer : Shadow
{
    public override void HandleDeath()
    {
        owner.Info.RemoveEffect(EffectType.IncreaseDef, this);
        owner.Info.RemoveEffect(EffectType.IncreaseSpd, this);
        base.HandleDeath();
    }
}