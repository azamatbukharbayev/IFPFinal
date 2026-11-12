namespace TaskFlow.Application.Features.Analytics;

using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Services;

public class AnalyticsService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public AnalyticsService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    
    public async Task<AnalyticsDto> GetAnalyticsAsync(CancellationToken ct = default)
    {
        var tasks = await _unitOfWork.Tasks.GetAllAsync(ct);
        var projects = await _unitOfWork.Projects.GetAllAsync(ct);
        var now = DateTime.UtcNow;
        
        var snapshot = TaskAnalyticsService.ComputeAnalytics(tasks.ToList(), now);
        
        var projectProgress = new List<ProjectProgressDto>();
        foreach (var project in projects)
        {
            var projectTasks = await _unitOfWork.Tasks.GetByProjectIdAsync(project.Id, ct);
            var progress = ProjectProgressService.CalculateProgress(projectTasks.ToList());
            projectProgress.Add(new ProjectProgressDto(
                project.Id, project.Name, progress.CompletionPercentage,
                progress.TotalTasks, progress.CompletedTasks));
        }
        
        return new AnalyticsDto(
            snapshot.TotalTasks, snapshot.CompletedTasks, snapshot.PendingTasks,
            snapshot.InProgressTasks, snapshot.OverdueTasks, snapshot.BlockedTasks,
            snapshot.CompletionRate, snapshot.AverageCompletionTimeHours,
            snapshot.OverdueRatio,
            snapshot.PriorityDistribution.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
            snapshot.StatusDistribution.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
            snapshot.WeeklyCompletions.Select(w => new WeeklyProductivityDto(w.WeekLabel, w.Count)).ToList(),
            projectProgress);
    }
    
    public async Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default)
    {
        var tasks = await _unitOfWork.Tasks.GetAllAsync(ct);
        var projects = await _unitOfWork.Projects.GetAllAsync(ct);
        var recentLogs = await _unitOfWork.ActivityLogs.GetRecentAsync(10, ct);
        var now = DateTime.UtcNow;
        
        var snapshot = TaskAnalyticsService.ComputeAnalytics(tasks.ToList(), now);
        
        var upcomingDeadlines = tasks
            .GetUpcomingDeadlines(now, 7)
            .Take(5)
            .Select(t => MapTaskDto(t, now))
            .ToList();
        
        var recentTaskIds = recentLogs
            .Where(l => l.TaskId.HasValue)
            .Select(l => l.TaskId!.Value)
            .Distinct()
            .Take(5)
            .ToList();
        
        var recentActivity = tasks
            .Where(t => recentTaskIds.Contains(t.Id))
            .Select(t => MapTaskDto(t, now))
            .ToList();
        
        var projectSummaries = new List<ProjectSummaryDto>();
        foreach (var project in projects)
        {
            var projectTasks = await _unitOfWork.Tasks.GetByProjectIdAsync(project.Id, ct);
            var progress = ProjectProgressService.CalculateProgress(projectTasks.ToList());
            projectSummaries.Add(new ProjectSummaryDto(project.Id, project.Name, progress.CompletionPercentage, progress.TotalTasks));
        }
        
        var weeklyProductivity = snapshot.WeeklyCompletions
            .Select(w => new WeeklyProductivityDto(w.WeekLabel, w.Count))
            .ToList();
        
        return new DashboardDto(
            snapshot.TotalTasks, snapshot.CompletedTasks, snapshot.PendingTasks,
            snapshot.InProgressTasks, snapshot.OverdueTasks, snapshot.BlockedTasks,
            snapshot.CompletionRate, snapshot.AverageCompletionTimeHours,
            upcomingDeadlines, recentActivity,
            projectSummaries, weeklyProductivity);
    }
    
    private static TaskItemDto MapTaskDto(TaskFlow.Domain.Entities.TaskItem task, DateTime now)
    {
        var score = TaskPrioritizationService.CalculatePriorityScore(task, now);
        var deadlineClass = TaskAnalyticsService.ClassifyDeadline(task.Deadline, now);
        return new TaskItemDto(
            task.Id, task.Title, task.Description,
            task.Status, task.Priority,
            task.CreatedAt, task.Deadline, task.CompletedAt,
            task.ProjectId, task.Project?.Name,
            task.EstimatedEffortHours,
            task.GetLabels(), task.ParentTaskId,
            task.DependsOn?.Select(d => d.DependsOnTaskId).ToList() ?? new List<Guid>(),
            score.Value, deadlineClass);
    }
}
