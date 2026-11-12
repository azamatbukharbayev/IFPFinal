namespace TaskFlow.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Persistence;

public class TaskRepository : ITaskRepository
{
    private readonly TaskFlowDbContext _context;
    
    public TaskRepository(TaskFlowDbContext context) => _context = context;
    
    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.DependsOn)
            .Include(t => t.SubTasks)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    
    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.DependsOn)
            .AsNoTracking()
            .ToListAsync(ct);
    
    public async Task<IReadOnlyList<TaskItem>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default) =>
        await _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.Project)
            .AsNoTracking()
            .ToListAsync(ct);
    
    public async Task<IReadOnlyList<TaskItem>> GetByStatusAsync(TaskItemStatus status, CancellationToken ct = default) =>
        await _context.Tasks
            .Where(t => t.Status == status)
            .Include(t => t.Project)
            .AsNoTracking()
            .ToListAsync(ct);
    
    public async Task<IReadOnlyList<TaskItem>> GetSubtasksAsync(Guid parentTaskId, CancellationToken ct = default) =>
        await _context.Tasks
            .Where(t => t.ParentTaskId == parentTaskId)
            .Include(t => t.Project)
            .AsNoTracking()
            .ToListAsync(ct);
    
    public async Task AddAsync(TaskItem task, CancellationToken ct = default) =>
        await _context.Tasks.AddAsync(task, ct);
    
    public Task UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        _context.Tasks.Update(task);
        return Task.CompletedTask;
    }
    
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var task = await _context.Tasks.FindAsync(new object[] { id }, ct);
        if (task is not null)
            _context.Tasks.Remove(task);
    }
    
    public async Task<IReadOnlyList<TaskDependency>> GetDependenciesAsync(CancellationToken ct = default) =>
        await _context.TaskDependencies.AsNoTracking().ToListAsync(ct);
    
    public async Task<IReadOnlyList<TaskDependency>> GetTaskDependenciesAsync(Guid taskId, CancellationToken ct = default) =>
        await _context.TaskDependencies
            .Where(d => d.TaskId == taskId)
            .AsNoTracking()
            .ToListAsync(ct);
    
    public async Task AddDependencyAsync(TaskDependency dependency, CancellationToken ct = default) =>
        await _context.TaskDependencies.AddAsync(dependency, ct);
    
    public async Task RemoveDependencyAsync(Guid dependencyId, CancellationToken ct = default)
    {
        var dep = await _context.TaskDependencies.FindAsync(new object[] { dependencyId }, ct);
        if (dep is not null)
            _context.TaskDependencies.Remove(dep);
    }
}
