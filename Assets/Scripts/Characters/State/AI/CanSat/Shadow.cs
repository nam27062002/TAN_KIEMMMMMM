public abstract class Shadow : AICharacter
{
    public Character owner;
    public override void OnDamageTaken(DamageTakenParams damageTakenParams)
    {
        Info.OnDamageTaken(damageTakenParams);
        if (damageTakenParams.Damage == 0)
        {
            ShowMessage("Né");
        }
        damageTakenParams.OnSetDamageTakenFinished?.Invoke(new FinishApplySkillParams()
        {
            Character = this,
            WaitForCounter = false,
        });
    }
    
    public override void HandleDeath()
    {
        Info.Cell.CellType = CellType.Walkable;
        Destroy(gameObject);
    }
}