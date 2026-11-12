namespace TaskFlow.Domain.Services;

using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Functional;

public static class TaskAnalyticsService
{
    public record AnalyticsSnapshot(
        int TotalTasks,
        int CompletedTasks,
        int PendingTasks,
        int InProgressTasks,
        int OverdueTasks,
        int BlockedTasks,
        double CompletionRate,
        double AverageCompletionTimeHours,
        double OverdueRatio,
        IReadOnlyDictionary<TaskPriority, int> PriorityDistribution,
        IReadOnlyDictionary<TaskItemStatus, int> StatusDistribution,
        IReadOnlyList<(string WeekLabel, int Count)> WeeklyCompletions);
    
    // Pure function: compute full analytics snapshot
    public static AnalyticsSnapshot ComputeAnalytics(IReadOnlyList<TaskItem> tasks, DateTime referenceTime)
    {
        var total = tasks.Count;
        var completed = tasks.Count(t => t.Status == TaskItemStatus.Completed);
        var pending = tasks.Count(t => t.Status == TaskItemStatus.ToDo);
        var inProgress = tasks.Count(t => t.Status == TaskItemStatus.InProgress);
        var blocked = tasks.Count(t => t.Status == TaskItemStatus.Blocked);
        var overdue = tasks.Count(t => t.Deadline.HasValue && t.Deadline < referenceTime && t.Status != TaskItemStatus.Completed);
        
        var completionRate = total > 0 ? (double)completed / total * 100.0 : 0.0;
        var overdueRatio = total > 0 ? (double)overdue / total * 100.0 : 0.0;
        
        var avgCompletion = tasks
            .Where(t => t.Status == TaskItemStatus.Completed && t.CompletedAt.HasValue)
            .Select(t => (t.CompletedAt!.Value - t.CreatedAt).TotalHours)
            .DefaultIfEmpty(0)
            .Average();
        
        var priorityDist = tasks
            .GroupBy(t => t.Priority)
            .ToDictionary(g => g.Key, g => g.Count());
        // Ensure all priorities present
        foreach (var p in Enum.GetValues<TaskPriority>())
            priorityDist.TryAdd(p, 0);
        
        var statusDist = tasks
            .GroupBy(t => t.Status)
            .ToDictionary(g => g.Key, g => g.Count());
        foreach (var s in Enum.GetValues<TaskItemStatus>())
            statusDist.TryAdd(s, 0);
        
        // Weekly completions for last 8 weeks
        var weeklyCompletions = Enumerable.Range(0, 8)
            .Select(i =>
            {
                var weekStart = referenceTime.AddDays(-7 * (7 - i)).Date;
                var weekEnd = weekStart.AddDays(7);
                var label = weekStart.ToString("MMM dd");
                var count = tasks.Count(t => t.CompletedAt.HasValue && t.CompletedAt.Value >= weekStart && t.CompletedAt.Value < weekEnd);
                return (WeekLabel: label, Count: count);
            })
            .ToList()
            .AsReadOnly();
        
        return new AnalyticsSnapshot(
            total, completed, pending, inProgress, overdue, blocked,
            completionRate, avgCompletion, overdueRatio,
            priorityDist, statusDist, weeklyCompletions);
    }
    
    // Pure function: classify deadline status
    public static string ClassifyDeadline(DateTime? deadline, DateTime referenceTime) =>
        deadline switch
        {
            null => "No Deadline",
            DateTime d when d < referenceTime => "Overdue",
            DateTime d when (d - referenceTime).TotalDays < 1 => "Due Today",
            DateTime d when (d - referenceTime).TotalDays < 3 => "Due Soon",
            DateTime d when (d - referenceTime).TotalDays < 7 => "This Week",
            DateTime d when (d - referenceTime).TotalDays < 30 => "This Month",
            _ => "Later"
        };
    
    // Higher-order: create a filter pipeline
    public static Func<IEnumerable<TaskItem>, IEnumerable<TaskItem>> CreateFilterPipeline(
        TaskItemStatus? status = null,
        TaskPriority? priority = null,
        Guid? projectId = null,
        string? searchTerm = null)
    {
        Func<IEnumerable<TaskItem>, IEnumerable<TaskItem>> pipeline = tasks => tasks;
        
        if (status.HasValue)
        {
            var s = status.Value;
            pipeline = pipeline.Then(tasks => tasks.Where(t => t.Status == s));
        }
        if (priority.HasValue)
        {
            var p = priority.Value;
            pipeline = pipeline.Then(tasks => tasks.Where(t => t.Priority == p));
        }
        if (projectId.HasValue)
        {
            var pid = projectId.Value;
            pipeline = pipeline.Then(tasks => tasks.Where(t => t.ProjectId == pid));
        }
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLowerInvariant();
            pipeline = pipeline.Then(tasks => tasks.Where(t => 
                t.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (t.Description?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)));
        }
        
        return pipeline;
    }
    
    // Compose: the Then extension for IEnumerable pipeline functions
    private static Func<IEnumerable<TaskItem>, IEnumerable<TaskItem>> Then(
        this Func<IEnumerable<TaskItem>, IEnumerable<TaskItem>> first,
        Func<IEnumerable<TaskItem>, IEnumerable<TaskItem>> second) =>
        input => second(first(input));
}
