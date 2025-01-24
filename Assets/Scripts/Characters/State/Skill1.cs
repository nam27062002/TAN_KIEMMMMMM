public class Skill1 : CharacterState
{
    public Skill1(Character character) : base(character)
    {
    }

    public override string NameState { get; set; } = "Đánh thường";
    
    public override void OnEnter()
    {
        base.OnEnter();
        Character.PlayAnim(AnimationParameterNameType.Skill1, OnEndAnim);
    }

    public override void OnExit()
    {
        
    }
}