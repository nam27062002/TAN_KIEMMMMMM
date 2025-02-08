using System.Linq;

public class LyVoDanh : PlayerCharacter
{
    protected override void SetStateMachine()
    {
        StateMachine = new CharacterStateMachine(this,
            new IdleState(this),
            new LVDMoveState(this),
            new DamageTakenState(this),
            new LVDSkillState(this));
    }

    public void TryTriggerFlyingTempest()
    {
        foreach (var item in PendingPassiveSkillsTrigger)
        {
            if (item is not FlyingTempest flyingTempest) continue;
            flyingTempest.OnTrigger();
            PendingPassiveSkillsTrigger.Remove(flyingTempest);
            break;
        }
    }
    
    protected override void SetSpeed()
    {
        if (GpManager.IsTutorialLevel)
        {
            CharacterInfo.Speed = 10;
        }
        else
        {
            base.SetSpeed();
        }
        CharacterInfo.Speed = 50;
    }       
    
}