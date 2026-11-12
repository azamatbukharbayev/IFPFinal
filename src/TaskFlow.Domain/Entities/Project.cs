namespace TaskFlow.Domain.Entities;

using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Functional;

public class Project
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProjectStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? Deadline { get; private set; }
    
    public ICollection<TaskItem> Tasks { get; private set; } = new List<TaskItem>();
    
    private Project() { }
    
    public static Result<Project> Create(string name, string? description, DateTime? deadline)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Project>.Failure("Project name is required.");
        if (name.Length > 200)
            return Result<Project>.Failure("Project name must be 200 characters or fewer.");
        
        return Result<Project>.Success(new Project
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Status = ProjectStatus.Active,
            CreatedAt = DateTime.UtcNow,
            Deadline = deadline
        });
    }
    
    public Result<TaskOperationResult> UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Result<TaskOperationResult>.Failure("Project name is required.");
        Name = newName.Trim();
        return Result<TaskOperationResult>.Success(new TaskOperationResult.Updated(Id, "Name"));
    }
    
    public Result<TaskOperationResult> UpdateDescription(string? newDescription)
    {
        Description = newDescription?.Trim();
        return Result<TaskOperationResult>.Success(new TaskOperationResult.Updated(Id, "Description"));
    }
    
    public Result<TaskOperationResult> UpdateDeadline(DateTime? newDeadline)
    {
        Deadline = newDeadline;
        return Result<TaskOperationResult>.Success(new TaskOperationResult.Updated(Id, "Deadline"));
    }
    
    public void SetStatus(ProjectStatus status) => Status = status;
}
