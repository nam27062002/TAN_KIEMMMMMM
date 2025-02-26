public class Assassin : Shadow
{
    public override void OnDie()
    {
        owner.Info.RemoveAllEffect(EffectType.IncreaseDamage);
        owner.Info.RemoveAllEffect(EffectType.ReduceHitChange);
        base.OnDie();
    }
}