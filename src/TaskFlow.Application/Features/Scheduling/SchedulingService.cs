namespace TaskFlow.Application.Features.Scheduling;

using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Services;

public class SchedulingService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public SchedulingService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    
    public async Task<IReadOnlyList<TaskItemDto>> GetTasksForDateRangeAsync(
        DateTime start, DateTime end, CancellationToken ct = default)
    {
        var tasks = await _unitOfWork.Tasks.GetAllAsync(ct);
        var now = DateTime.UtcNow;
        
        return tasks
            .Where(t => t.Deadline.HasValue && t.Deadline.Value >= start && t.Deadline.Value <= end)
            .OrderBy(t => t.Deadline)
            .Select(t =>
            {
                var score = TaskPrioritizationService.CalculatePriorityScore(t, now);
                var deadlineClass = TaskAnalyticsService.ClassifyDeadline(t.Deadline, now);
                return new TaskItemDto(
                    t.Id, t.Title, t.Description, t.Status, t.Priority,
                    t.CreatedAt, t.Deadline, t.CompletedAt,
                    t.ProjectId, t.Project?.Name, t.EstimatedEffortHours,
                    t.GetLabels(), t.ParentTaskId,
                    t.DependsOn?.Select(d => d.DependsOnTaskId).ToList() ?? new List<Guid>(),
                    score.Value, deadlineClass);
            })
            .ToList()
            .AsReadOnly();
    }
    
    public async Task<Dictionary<DateTime, List<TaskItemDto>>> GetMonthlyCalendarAsync(
        int year, int month, CancellationToken ct = default)
    {
        var firstDay = new DateTime(year, month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);
        
        var tasks = await GetTasksForDateRangeAsync(firstDay, lastDay.AddDays(1), ct);
        
        return tasks
            .Where(t => t.Deadline.HasValue)
            .GroupBy(t => t.Deadline!.Value.Date)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
