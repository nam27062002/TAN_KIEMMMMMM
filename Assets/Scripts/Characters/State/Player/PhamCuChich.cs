public class PhamCuChich : PlayerCharacter
{
    public Cell CurrentShield;
    protected override void SetStateMachine()
    {
        StateMachine = new CharacterStateMachine(this,
            new IdleState(this),
            new MoveState(this),
            new PlayerDamageTakenState(this),
            new PhamCuChich_SkillState(this));
    }
    
    public override void HandleMpChanged(int value)
    {
        if (value == 0) return;
        if (CanUseHp(value, out var hp))
        {
            Info.CurrentHp += hp;
            Info.OnHpChangedInvoke(hp);
            Info.CurrentMp += hp;
            Info.OnMpChangedInvoke(hp);
        }
        else
        {
            var dragon = Info.DragonArmorEffectData;
            if (dragon != null)
            {
                if (dragon.actor != null)
                {
                    value = Utils.RoundNumber(value * 1f / 2f);
                    dragon.actor.HandleMpChanged(value);
                }
            }

            Info.CurrentMp += value;
            Info.OnMpChangedInvoke(value);
        }
    }

    private bool CanUseHp(int value, out int hp)
    {
        if (!Info.IsToggleOn)
        {
            hp = 0;
            return false;
        }
        hp = Utils.RoundNumber(value / 2f);
        return Info.CurrentHp > -hp;
    }
}