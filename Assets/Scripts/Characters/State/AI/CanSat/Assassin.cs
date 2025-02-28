public class Assassin : Shadow
{
    public override void OnDie()
    {
        owner.Info.RemoveEffect(EffectType.IncreaseDamage, this);
        owner.Info.RemoveEffect(EffectType.ReduceHitChange, this);
        base.OnDie();
    }
}