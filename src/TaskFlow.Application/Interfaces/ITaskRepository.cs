namespace TaskFlow.Application.Interfaces;

using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TaskItem>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
    Task<IReadOnlyList<TaskItem>> GetByStatusAsync(TaskItemStatus status, CancellationToken ct = default);
    Task<IReadOnlyList<TaskItem>> GetSubtasksAsync(Guid parentTaskId, CancellationToken ct = default);
    Task AddAsync(TaskItem task, CancellationToken ct = default);
    Task UpdateAsync(TaskItem task, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TaskDependency>> GetDependenciesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TaskDependency>> GetTaskDependenciesAsync(Guid taskId, CancellationToken ct = default);
    Task AddDependencyAsync(TaskDependency dependency, CancellationToken ct = default);
    Task RemoveDependencyAsync(Guid dependencyId, CancellationToken ct = default);
}
