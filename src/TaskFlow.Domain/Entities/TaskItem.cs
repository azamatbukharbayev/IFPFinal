namespace TaskFlow.Domain.Entities;

using TaskFlow.Domain.Enums;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.Functional;

public class TaskItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? Deadline { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid? ProjectId { get; private set; }
    public decimal EstimatedEffortHours { get; private set; }
    public string LabelsJson { get; private set; } = "[]";
    public Guid? ParentTaskId { get; private set; }
    
    // Navigation properties
    public Project? Project { get; private set; }
    public TaskItem? ParentTask { get; private set; }
    public ICollection<TaskItem> SubTasks { get; private set; } = new List<TaskItem>();
    public ICollection<TaskDependency> DependsOn { get; private set; } = new List<TaskDependency>();
    public ICollection<TaskDependency> DependedOnBy { get; private set; } = new List<TaskDependency>();
    public ICollection<ActivityLog> ActivityLogs { get; private set; } = new List<ActivityLog>();
    
    private TaskItem() { } // EF Core constructor
    
    public static Result<TaskItem> Create(string title, string? description, TaskPriority priority, 
        DateTime? deadline, Guid? projectId, decimal estimatedEffortHours, List<string>? labels,
        Guid? parentTaskId = null)
    {
        var validation = Validate(title, estimatedEffortHours);
        if (!validation.IsValid)
            return Result<TaskItem>.Failure(validation.Errors);
        
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description?.Trim(),
            Status = TaskItemStatus.ToDo,
            Priority = priority,
            CreatedAt = DateTime.UtcNow,
            Deadline = deadline,
            ProjectId = projectId,
            EstimatedEffortHours = estimatedEffortHours,
            LabelsJson = System.Text.Json.JsonSerializer.Serialize(labels ?? new List<string>()),
            ParentTaskId = parentTaskId
        };
        return Result<TaskItem>.Success(task);
    }
    
    public Result<TaskOperationResult> UpdateTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            return Result<TaskOperationResult>.Failure("Title cannot be empty.");
        Title = newTitle.Trim();
        return Result<TaskOperationResult>.Success(new TaskOperationResult.Updated(Id, "Title"));
    }
    
    public Result<TaskOperationResult> UpdateDescription(string? newDescription)
    {
        Description = newDescription?.Trim();
        return Result<TaskOperationResult>.Success(new TaskOperationResult.Updated(Id, "Description"));
    }
    
    public Result<TaskOperationResult> UpdatePriority(TaskPriority newPriority)
    {
        Priority = newPriority;
        return Result<TaskOperationResult>.Success(new TaskOperationResult.Updated(Id, "Priority"));
    }
    
    public Result<TaskOperationResult> UpdateDeadline(DateTime? newDeadline)
    {
        Deadline = newDeadline;
        return Result<TaskOperationResult>.Success(new TaskOperationResult.Updated(Id, "Deadline"));
    }
    
    public Result<TaskOperationResult> UpdateEffort(decimal hours)
    {
        if (hours < 0)
            return Result<TaskOperationResult>.Failure("Effort cannot be negative.");
        EstimatedEffortHours = hours;
        return Result<TaskOperationResult>.Success(new TaskOperationResult.Updated(Id, "EstimatedEffort"));
    }
    
    public Result<TaskOperationResult> UpdateLabels(List<string> labels)
    {
        LabelsJson = System.Text.Json.JsonSerializer.Serialize(labels);
        return Result<TaskOperationResult>.Success(new TaskOperationResult.Updated(Id, "Labels"));
    }
    
    public Result<TaskOperationResult> UpdateProject(Guid? projectId)
    {
        ProjectId = projectId;
        return Result<TaskOperationResult>.Success(new TaskOperationResult.Updated(Id, "ProjectId"));
    }
    
    public Result<TaskOperationResult> ChangeStatus(TaskItemStatus newStatus)
    {
        var transitionResult = ValidateStatusTransition(Status, newStatus);
        if (transitionResult.IsFailure)
            return Result<TaskOperationResult>.Failure(transitionResult.Errors);
        
        var oldStatus = Status.ToString();
        Status = newStatus;
        if (newStatus == TaskItemStatus.Completed)
            CompletedAt = DateTime.UtcNow;
        else
            CompletedAt = null;
        
        return Result<TaskOperationResult>.Success(
            new TaskOperationResult.StatusChanged(Id, oldStatus, newStatus.ToString()));
    }
    
    public List<string> GetLabels() =>
        System.Text.Json.JsonSerializer.Deserialize<List<string>>(LabelsJson) ?? new List<string>();
    
    // Pure validation function
    public static ValidationResult Validate(string title, decimal estimatedEffortHours)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(title))
            errors.Add("Title is required.");
        if (title?.Length > 200)
            errors.Add("Title must be 200 characters or fewer.");
        if (estimatedEffortHours < 0)
            errors.Add("Estimated effort cannot be negative.");
        return errors.Count == 0 ? ValidationResult.Valid() : ValidationResult.Invalid(errors.ToArray());
    }
    
    // Pure function: validate status transition
    public static Result<TaskItemStatus> ValidateStatusTransition(TaskItemStatus current, TaskItemStatus target) =>
        (current, target) switch
        {
            (TaskItemStatus.ToDo, TaskItemStatus.InProgress) => Result<TaskItemStatus>.Success(target),
            (TaskItemStatus.ToDo, TaskItemStatus.Blocked) => Result<TaskItemStatus>.Success(target),
            (TaskItemStatus.ToDo, TaskItemStatus.Completed) => Result<TaskItemStatus>.Success(target),
            (TaskItemStatus.InProgress, TaskItemStatus.Blocked) => Result<TaskItemStatus>.Success(target),
            (TaskItemStatus.InProgress, TaskItemStatus.Completed) => Result<TaskItemStatus>.Success(target),
            (TaskItemStatus.InProgress, TaskItemStatus.ToDo) => Result<TaskItemStatus>.Success(target),
            (TaskItemStatus.Blocked, TaskItemStatus.ToDo) => Result<TaskItemStatus>.Success(target),
            (TaskItemStatus.Blocked, TaskItemStatus.InProgress) => Result<TaskItemStatus>.Success(target),
            (TaskItemStatus.Completed, TaskItemStatus.ToDo) => Result<TaskItemStatus>.Success(target),
            (var c, var t) when c == t => Result<TaskItemStatus>.Failure($"Task is already in {c} status."),
            _ => Result<TaskItemStatus>.Failure($"Invalid transition from {current} to {target}.")
        };
}
