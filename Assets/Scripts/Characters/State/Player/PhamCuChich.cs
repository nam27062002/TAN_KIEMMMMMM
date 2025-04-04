using System;
using System.Linq;
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
            _damage = Utils.RoundNumber(_damageLostInCurrentRound * 0.3f);
            Info.Attributes.atk += _damage;
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{characterConfig.characterName}] Bất Động Như Sơn: Áp dụng {_damage} damage từ máu đã mất ở round trước (30%)");
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
    
    // Thêm phương thức để kiểm tra khả năng sử dụng skill với mode toggle
    public bool CheckCanCastSkillWithToggle(SkillInfo skillInfo)
    {
        if (!Info.IsToggleOn)
        {
            // Nếu toggle tắt, sử dụng logic mặc định
            return Info.CanCastSkill(skillInfo);
        }
        
        // Nếu toggle bật, kiểm tra cả HP và MP
        int requiredMp = skillInfo.mpCost;
        int halfMp = Utils.RoundNumber(requiredMp / 2f);
        int halfHp = Utils.RoundNumber(requiredMp / 2f);
        
        // Kiểm tra có đủ MP và HP không
        bool enoughMp = Info.CurrentMp >= halfMp;
        bool enoughHp = Info.CurrentHp > halfHp;
        return enoughMp && enoughHp && Info.ActionPointsList.Any(point => point == 3);
    }
}