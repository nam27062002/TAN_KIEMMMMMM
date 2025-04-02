using System;
public class PhamCuChich : PlayerCharacter
{
    public Cell CurrentShield;
    private int _damage;
    private int _damageLostInCurrentRound;
    
    protected override void SetStateMachine()
    {
        StateMachine = new CharacterStateMachine(this,
            new IdleState(this),
            new MoveState(this),
            new PlayerDamageTakenState(this),
            new PhamCuChich_SkillState(this));
    }
    
    public override void Initialize(Cell cell, int iD)
    {
        base.Initialize(cell, iD);
        Info.OnReduceHp += InfoOnOnReduceHp;
    }

    public override void HandleDeath()
    {
        Info.OnReduceHp -= InfoOnOnReduceHp;
        base.HandleDeath();
    }
    
    protected override void OnNewRound(object sender, EventArgs e)
    {
        if (_damage != 0)
        {
            Info.Attributes.atk -= _damage;
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{characterConfig.characterName}] Bất Động Như Sơn: new round => Reset damage");
            _damage = 0;
        }
        
        if (_damageLostInCurrentRound > 0)
        {
            _damage = _damageLostInCurrentRound;
            Info.Attributes.atk += _damage;
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{characterConfig.characterName}] Bất Động Như Sơn: Áp dụng {_damage} damage từ máu đã mất ở round trước");
            _damageLostInCurrentRound = 0;
        }
    }

    private void InfoOnOnReduceHp(object sender, int damage)
    {
        if (Info == null) return;
        _damageLostInCurrentRound += damage;
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{characterConfig.characterName}] Bất Động Như Sơn: Lưu {damage} damage, tổng {_damageLostInCurrentRound} (sẽ áp dụng ở round tiếp theo)");
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
                if (dragon.Actor != null)
                {
                    value = Utils.RoundNumber(value * 1f / 2f);
                    dragon.Actor.HandleMpChanged(value);
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