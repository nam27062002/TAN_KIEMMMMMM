public interface IState
{
    public string NameState { get; set; } 
    public void OnEnter(StateParams stateParams = null);
    public void OnExit();
}