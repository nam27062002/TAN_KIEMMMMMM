public abstract class Shadow : AICharacter
{
    public override void OnDamageTaken(DamageTakenParams damageTakenParams)
    {
        ChangeState(ECharacterState.DamageTaken, damageTakenParams);
        damageTakenParams.OnSetDamageTakenFinished?.Invoke(new FinishApplySkillParams()
        {
            Character = this,
            WaitForCounter = false,
        });
    }
}