namespace TaskFlow.Domain.Functional;

public abstract record TaskOperationResult
{
    public sealed record Created(Guid TaskId, string Title) : TaskOperationResult;
    public sealed record Updated(Guid TaskId, string FieldChanged) : TaskOperationResult;
    public sealed record Deleted(Guid TaskId) : TaskOperationResult;
    public sealed record StatusChanged(Guid TaskId, string OldStatus, string NewStatus) : TaskOperationResult;
    public sealed record OperationFailed(Guid TaskId, string Reason) : TaskOperationResult;
    
    public string Describe() => this switch
    {
        Created c => $"Task '{c.Title}' created with ID {c.TaskId}",
        Updated u => $"Task {u.TaskId}: '{u.FieldChanged}' updated",
        Deleted d => $"Task {d.TaskId} deleted",
        StatusChanged s => $"Task {s.TaskId}: status changed from {s.OldStatus} to {s.NewStatus}",
        OperationFailed f => $"Task {f.TaskId}: operation failed - {f.Reason}",
        _ => "Unknown operation"
    };
}
