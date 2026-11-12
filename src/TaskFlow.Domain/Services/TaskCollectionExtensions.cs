namespace TaskFlow.Domain.Services;

using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

public static class TaskCollectionExtensions
{
    public static IEnumerable<TaskItem> GetOverdueTasks(this IEnumerable<TaskItem> tasks, DateTime referenceTime) =>
        tasks.Where(t => t.Deadline.HasValue && t.Deadline < referenceTime && t.Status != TaskItemStatus.Completed);
    
    public static IEnumerable<TaskItem> GetCompletedTasks(this IEnumerable<TaskItem> tasks) =>
        tasks.Where(t => t.Status == TaskItemStatus.Completed);
    
    public static IEnumerable<TaskItem> GetUpcomingDeadlines(this IEnumerable<TaskItem> tasks, DateTime referenceTime, int days = 7) =>
        tasks.Where(t => t.Deadline.HasValue && t.Deadline > referenceTime && t.Deadline <= referenceTime.AddDays(days) && t.Status != TaskItemStatus.Completed)
             .OrderBy(t => t.Deadline);
    
    public static IEnumerable<TaskItem> OrderByUrgency(this IEnumerable<TaskItem> tasks, DateTime referenceTime) =>
        tasks.OrderByDescending(t => TaskPrioritizationService.CalculatePriorityScore(t, referenceTime).Value);
    
    public static IEnumerable<IGrouping<Guid?, TaskItem>> GroupByProject(this IEnumerable<TaskItem> tasks) =>
        tasks.GroupBy(t => t.ProjectId);
    
    public static IEnumerable<IGrouping<TaskItemStatus, TaskItem>> GroupByStatus(this IEnumerable<TaskItem> tasks) =>
        tasks.GroupBy(t => t.Status);
    
    public static IEnumerable<IGrouping<TaskPriority, TaskItem>> GroupByPriority(this IEnumerable<TaskItem> tasks) =>
        tasks.GroupBy(t => t.Priority);
    
    public static decimal TotalEstimatedEffort(this IEnumerable<TaskItem> tasks) =>
        tasks.Sum(t => t.EstimatedEffortHours);
    
    public static IEnumerable<TaskItem> WithStatus(this IEnumerable<TaskItem> tasks, TaskItemStatus status) =>
        tasks.Where(t => t.Status == status);
    
    public static IEnumerable<TaskItem> WithPriority(this IEnumerable<TaskItem> tasks, TaskPriority priority) =>
        tasks.Where(t => t.Priority == priority);
    
    // Partial application demo: create status-specific filters
    public static Func<IEnumerable<TaskItem>, IEnumerable<TaskItem>> FilterByStatus(TaskItemStatus status) =>
        tasks => tasks.Where(t => t.Status == status);
    
    public static Func<IEnumerable<TaskItem>, IEnumerable<TaskItem>> FilterByPriority(TaskPriority priority) =>
        tasks => tasks.Where(t => t.Priority == priority);
}
