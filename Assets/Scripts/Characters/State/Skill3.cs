public class Skill3 : CharacterState
{
    public Skill3(Character character) : base(character)
    {
    }

    public override string NameState { get; set; } = "Skill 2";
    
    public override void OnEnter()
    {
        base.OnEnter();
        Character.PlayAnim(AnimationParameterNameType.Skill3, OnEndAnim);
    }

    public override void OnExit()
    {
        
    }
}