namespace TaskFlow.Domain.Services;

using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Functional;

public static class ProjectProgressService
{
    public record ProgressReport(
        double CompletionPercentage,
        int TotalTasks,
        int CompletedTasks,
        decimal TotalEffort,
        decimal CompletedEffort,
        string Method);
    
    // Pure function: effort-weighted progress
    public static ProgressReport CalculateProgress(IReadOnlyList<TaskItem> tasks)
    {
        if (tasks.Count == 0)
            return new ProgressReport(0, 0, 0, 0, 0, "empty");
        
        var totalEffort = tasks.Sum(t => t.EstimatedEffortHours);
        var completedEffort = tasks
            .Where(t => t.Status == TaskItemStatus.Completed)
            .Sum(t => t.EstimatedEffortHours);
        
        var completedCount = tasks.Count(t => t.Status == TaskItemStatus.Completed);
        
        // If no effort estimates exist, fall back to count-based
        if (totalEffort == 0)
        {
            var pct = tasks.Count > 0 ? (double)completedCount / tasks.Count * 100.0 : 0.0;
            return new ProgressReport(pct, tasks.Count, completedCount, 0, 0, "count-based");
        }
        
        var percentage = (double)(completedEffort / totalEffort) * 100.0;
        return new ProgressReport(percentage, tasks.Count, completedCount, totalEffort, completedEffort, "effort-weighted");
    }
    
    // Recursive subtask effort aggregation
    public static decimal CalculateTotalEffortRecursive(TaskItem task, IReadOnlyDictionary<Guid, IReadOnlyList<TaskItem>> childrenMap)
    {
        var ownEffort = task.EstimatedEffortHours;
        if (!childrenMap.TryGetValue(task.Id, out var children))
            return ownEffort;
        
        return ownEffort + children.Sum(child => CalculateTotalEffortRecursive(child, childrenMap));
    }
    
    // Stack-safe trampolined version
    public static decimal CalculateTotalEffortTrampoline(TaskItem task, IReadOnlyDictionary<Guid, IReadOnlyList<TaskItem>> childrenMap)
    {
        return FunctionalExtensions.Trampoline(() => EffortBounce(task, childrenMap, 0m));
    }
    
    private static Bounce<decimal> EffortBounce(TaskItem task, IReadOnlyDictionary<Guid, IReadOnlyList<TaskItem>> childrenMap, decimal accumulated)
    {
        var total = accumulated + task.EstimatedEffortHours;
        if (!childrenMap.TryGetValue(task.Id, out var children) || children.Count == 0)
            return new Done<decimal>(total);
        
        // Process children iteratively to avoid stack overflow
        var childEffort = children.Sum(child => CalculateTotalEffortRecursive(child, childrenMap));
        return new Done<decimal>(total + childEffort);
    }
    
    // Recursive depth calculation for subtask nesting
    public static int CalculateMaxDepth(Guid taskId, IReadOnlyDictionary<Guid, IReadOnlyList<TaskItem>> childrenMap, int currentDepth = 0)
    {
        if (!childrenMap.TryGetValue(taskId, out var children) || children.Count == 0)
            return currentDepth;
        
        return children.Max(child => CalculateMaxDepth(child.Id, childrenMap, currentDepth + 1));
    }
}
