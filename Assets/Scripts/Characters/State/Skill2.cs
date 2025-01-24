public class Skill2 : CharacterState
{
    public Skill2(Character character) : base(character)
    {
    }

    public override string NameState { get; set; } = "Skill 1";
    
    public override void OnEnter()
    {
        base.OnEnter();
        Character.PlayAnim(AnimationParameterNameType.Skill2, OnEndAnim);
    }

    public override void OnExit()
    {
        
    }
}