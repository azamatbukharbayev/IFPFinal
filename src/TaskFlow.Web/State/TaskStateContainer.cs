namespace TaskFlow.Web.State;

public class TaskStateContainer
{
    public event Action? OnChange;

    public void NotifyStateChanged() => OnChange?.Invoke();
}
