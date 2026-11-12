namespace TaskFlow.Web.Services;

public enum ToastLevel { Info, Success, Warning, Error }

public record ToastMessage(string Title, string Message, ToastLevel Level, DateTime Timestamp);

public class ToastService
{
    public event Action<ToastMessage>? OnShow;

    public void ShowSuccess(string message, string title = "Success") =>
        OnShow?.Invoke(new ToastMessage(title, message, ToastLevel.Success, DateTime.UtcNow));

    public void ShowError(string message, string title = "Error") =>
        OnShow?.Invoke(new ToastMessage(title, message, ToastLevel.Error, DateTime.UtcNow));

    public void ShowWarning(string message, string title = "Warning") =>
        OnShow?.Invoke(new ToastMessage(title, message, ToastLevel.Warning, DateTime.UtcNow));

    public void ShowInfo(string message, string title = "Info") =>
        OnShow?.Invoke(new ToastMessage(title, message, ToastLevel.Info, DateTime.UtcNow));
}
