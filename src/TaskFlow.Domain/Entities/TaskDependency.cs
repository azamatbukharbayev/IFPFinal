namespace TaskFlow.Domain.Entities;

public class TaskDependency
{
    public Guid Id { get; private set; }
    public Guid TaskId { get; private set; }
    public Guid DependsOnTaskId { get; private set; }
    
    public TaskItem? Task { get; private set; }
    public TaskItem? DependsOnTask { get; private set; }
    
    private TaskDependency() { }
    
    public static TaskDependency Create(Guid taskId, Guid dependsOnTaskId)
    {
        if (taskId == dependsOnTaskId)
            throw new ArgumentException("A task cannot depend on itself.");
        return new TaskDependency
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            DependsOnTaskId = dependsOnTaskId
        };
    }
}
