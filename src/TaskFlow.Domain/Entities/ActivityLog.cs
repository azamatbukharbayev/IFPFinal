namespace TaskFlow.Domain.Entities;

public class ActivityLog
{
    public Guid Id { get; private set; }
    public Guid? TaskId { get; private set; }
    public Guid? ProjectId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Details { get; private set; } = string.Empty;
    public DateTime Timestamp { get; private set; }
    
    public TaskItem? Task { get; private set; }
    
    private ActivityLog() { }
    
    public static ActivityLog Create(string action, string details, Guid? taskId = null, Guid? projectId = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Action = action,
            Details = details,
            TaskId = taskId,
            ProjectId = projectId,
            Timestamp = DateTime.UtcNow
        };
}
