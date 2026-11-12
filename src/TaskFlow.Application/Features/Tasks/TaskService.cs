namespace TaskFlow.Application.Features.Tasks;

using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Functional;
using TaskFlow.Domain.Services;

public class TaskService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public TaskService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    
    public async Task<Result<TaskItemDto>> CreateTaskAsync(CreateTaskDto dto, CancellationToken ct = default)
    {
        var result = TaskItem.Create(
            dto.Title, dto.Description, dto.Priority,
            dto.Deadline, dto.ProjectId, dto.EstimatedEffortHours,
            dto.Labels, dto.ParentTaskId);
        
        return await result.Match<Task<Result<TaskItemDto>>>(
            async task =>
            {
                await _unitOfWork.Tasks.AddAsync(task, ct);
                await _unitOfWork.ActivityLogs.AddAsync(
                    ActivityLog.Create("TaskCreated", $"Task '{task.Title}' created", task.Id, task.ProjectId), ct);
                await _unitOfWork.SaveChangesAsync(ct);
                return Result<TaskItemDto>.Success(MapToDto(task));
            },
            errors => Task.FromResult(Result<TaskItemDto>.Failure(errors)));
    }
    
    public async Task<Result<TaskItemDto>> UpdateTaskAsync(UpdateTaskDto dto, CancellationToken ct = default)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(dto.Id, ct);
        if (task is null)
            return Result<TaskItemDto>.Failure($"Task with ID {dto.Id} not found.");
        
        var results = new List<Result<TaskOperationResult>>
        {
            task.UpdateTitle(dto.Title),
            task.UpdateDescription(dto.Description),
            task.UpdatePriority(dto.Priority),
            task.UpdateDeadline(dto.Deadline),
            task.UpdateEffort(dto.EstimatedEffortHours),
            task.UpdateLabels(dto.Labels ?? new List<string>()),
            task.UpdateProject(dto.ProjectId)
        };
        
        var errors = results.Where(r => r.IsFailure).SelectMany(r => r.Errors).ToList();
        if (errors.Count > 0)
            return Result<TaskItemDto>.Failure(errors);
        
        await _unitOfWork.Tasks.UpdateAsync(task, ct);
        await _unitOfWork.ActivityLogs.AddAsync(
            ActivityLog.Create("TaskUpdated", $"Task '{task.Title}' updated", task.Id, task.ProjectId), ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<TaskItemDto>.Success(MapToDto(task));
    }
    
    public async Task<Result<bool>> DeleteTaskAsync(Guid id, CancellationToken ct = default)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(id, ct);
        if (task is null)
            return Result<bool>.Failure($"Task with ID {id} not found.");
        
        await _unitOfWork.Tasks.DeleteAsync(id, ct);
        await _unitOfWork.ActivityLogs.AddAsync(
            ActivityLog.Create("TaskDeleted", $"Task '{task.Title}' deleted", id, task.ProjectId), ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
    
    public async Task<Result<TaskItemDto>> ChangeStatusAsync(Guid taskId, TaskItemStatus newStatus, CancellationToken ct = default)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(taskId, ct);
        if (task is null)
            return Result<TaskItemDto>.Failure($"Task with ID {taskId} not found.");
        
        // Check dependencies if completing
        if (newStatus == TaskItemStatus.Completed)
        {
            var allDeps = await _unitOfWork.Tasks.GetDependenciesAsync(ct);
            var allTasks = await _unitOfWork.Tasks.GetAllAsync(ct);
            var canComplete = DependencyGraphService.CanCompleteTask(taskId, allDeps, allTasks);
            if (canComplete.IsFailure)
                return Result<TaskItemDto>.Failure(canComplete.Errors);
        }
        
        var result = task.ChangeStatus(newStatus);
        return await result.Match<Task<Result<TaskItemDto>>>(
            async opResult =>
            {
                await _unitOfWork.Tasks.UpdateAsync(task, ct);
                await _unitOfWork.ActivityLogs.AddAsync(
                    ActivityLog.Create("StatusChanged", opResult.Describe(), taskId, task.ProjectId), ct);
                await _unitOfWork.SaveChangesAsync(ct);
                return Result<TaskItemDto>.Success(MapToDto(task));
            },
            errors => Task.FromResult(Result<TaskItemDto>.Failure(errors)));
    }
    
    public async Task<IReadOnlyList<TaskItemDto>> GetAllTasksAsync(
        TaskItemStatus? status = null, TaskPriority? priority = null,
        Guid? projectId = null, string? searchTerm = null,
        string? sortBy = null, CancellationToken ct = default)
    {
        var tasks = await _unitOfWork.Tasks.GetAllAsync(ct);
        var now = DateTime.UtcNow;
        
        // Build filter pipeline using higher-order functions
        var filter = TaskAnalyticsService.CreateFilterPipeline(status, priority, projectId, searchTerm);
        var filtered = filter(tasks);
        
        // Sort
        var sorted = sortBy?.ToLowerInvariant() switch
        {
            "deadline" => filtered.OrderBy(t => t.Deadline ?? DateTime.MaxValue),
            "created" => filtered.OrderByDescending(t => t.CreatedAt),
            "priority" => filtered.OrderByDescending(t => t.Priority),
            "urgency" => filtered.OrderByUrgency(now),
            _ => filtered.OrderByDescending(t => t.CreatedAt)
        };
        
        return sorted.Select(t => MapToDto(t, now)).ToList().AsReadOnly();
    }
    
    public async Task<TaskItemDto?> GetTaskByIdAsync(Guid id, CancellationToken ct = default)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(id, ct);
        return task is null ? null : MapToDto(task);
    }
    
    public async Task<IReadOnlyList<TaskItemDto>> GetSubtasksAsync(Guid parentTaskId, CancellationToken ct = default)
    {
        var subtasks = await _unitOfWork.Tasks.GetSubtasksAsync(parentTaskId, ct);
        return subtasks.Select(t => MapToDto(t)).ToList().AsReadOnly();
    }
    
    public async Task<Result<bool>> AddDependencyAsync(Guid taskId, Guid dependsOnTaskId, CancellationToken ct = default)
    {
        var allDeps = await _unitOfWork.Tasks.GetDependenciesAsync(ct);
        var validationResult = DependencyGraphService.ValidateNewDependency(taskId, dependsOnTaskId, allDeps);
        
        return await validationResult.Match<Task<Result<bool>>>(
            async dep =>
            {
                await _unitOfWork.Tasks.AddDependencyAsync(dep, ct);
                await _unitOfWork.ActivityLogs.AddAsync(
                    ActivityLog.Create("DependencyAdded", $"Dependency added: {taskId} depends on {dependsOnTaskId}", taskId), ct);
                await _unitOfWork.SaveChangesAsync(ct);
                return Result<bool>.Success(true);
            },
            errors => Task.FromResult(Result<bool>.Failure(errors)));
    }
    
    public async Task<Result<bool>> RemoveDependencyAsync(Guid dependencyId, CancellationToken ct = default)
    {
        await _unitOfWork.Tasks.RemoveDependencyAsync(dependencyId, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
    
    public async Task<IReadOnlyList<(TaskItemDto Task, double Score)>> GetPrioritizedTasksAsync(CancellationToken ct = default)
    {
        var tasks = await _unitOfWork.Tasks.GetAllAsync(ct);
        var now = DateTime.UtcNow;
        var activeTasks = tasks.Where(t => t.Status != TaskItemStatus.Completed).ToList();
        var ranked = TaskPrioritizationService.RankTasks(activeTasks, now);
        return ranked.Select(r => (MapToDto(r.Task, now), r.Score.Value)).ToList().AsReadOnly();
    }
    
    private static TaskItemDto MapToDto(TaskItem task, DateTime? referenceTime = null)
    {
        var now = referenceTime ?? DateTime.UtcNow;
        var score = TaskPrioritizationService.CalculatePriorityScore(task, now);
        var deadlineClass = TaskAnalyticsService.ClassifyDeadline(task.Deadline, now);
        var deps = task.DependsOn?.Select(d => d.DependsOnTaskId).ToList() ?? new List<Guid>();
        
        return new TaskItemDto(
            task.Id, task.Title, task.Description,
            task.Status, task.Priority,
            task.CreatedAt, task.Deadline, task.CompletedAt,
            task.ProjectId, task.Project?.Name,
            task.EstimatedEffortHours,
            task.GetLabels(), task.ParentTaskId, deps,
            score.Value, deadlineClass);
    }
}
