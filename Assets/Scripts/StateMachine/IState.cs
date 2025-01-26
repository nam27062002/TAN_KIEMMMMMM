public interface IState
{
    public string NameState { get; set; } 
    public void OnEnter();
    public void OnExit();
}