public interface ISubState
{
    bool enabled { get; set; }

    public void OnEnter();
    public void OnExit();
    public void OnUpdate();
}
