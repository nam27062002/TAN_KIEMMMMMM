public class Assassin : Shadow
{
    public override void HandleDeath()
    {
        owner.Info.RemoveEffect(EffectType.IncreaseDamage, this);
        owner.Info.RemoveEffect(EffectType.ReduceHitChange, this);
        base.HandleDeath();
    }
}