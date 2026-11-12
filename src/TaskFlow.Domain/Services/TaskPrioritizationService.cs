namespace TaskFlow.Domain.Services;

using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.Functional;

public static class TaskPrioritizationService
{
    public record PriorityWeights(double UrgencyWeight = 0.4, double ImportanceWeight = 0.35, double DeadlineWeight = 0.25);
    
    // Pure function: calculate urgency based on deadline proximity  
    public static double CalculateUrgency(DateTime? deadline, DateTime referenceTime)
    {
        if (deadline is null) return 30.0; // No deadline = moderate urgency
        var daysUntil = (deadline.Value - referenceTime).TotalDays;
        return daysUntil switch
        {
            < 0 => 100.0,   // Overdue
            < 1 => 95.0,    // Due today
            < 3 => 80.0,    // Very urgent
            < 7 => 60.0,    // This week
            < 14 => 40.0,   // Next two weeks
            < 30 => 20.0,   // This month
            _ => 10.0       // Far away
        };
    }
    
    // Pure function: map priority enum to importance score
    public static double CalculateImportance(TaskPriority priority) => priority switch
    {
        TaskPriority.Critical => 100.0,
        TaskPriority.High => 75.0,
        TaskPriority.Medium => 50.0,
        TaskPriority.Low => 25.0,
        _ => 0.0
    };
    
    // Pure function: deadline proximity factor
    public static double CalculateDeadlineProximity(DateTime? deadline, DateTime referenceTime)
    {
        if (deadline is null) return 0.0;
        var daysUntil = (deadline.Value - referenceTime).TotalDays;
        if (daysUntil <= 0) return 100.0;
        return Math.Max(0, 100.0 - (daysUntil * 2.0));
    }
    
    // Pure composite function
    public static PriorityScore CalculatePriorityScore(TaskItem task, DateTime referenceTime, PriorityWeights? weights = null)
    {
        var w = weights ?? new PriorityWeights();
        var urgency = CalculateUrgency(task.Deadline, referenceTime);
        var importance = CalculateImportance(task.Priority);
        var deadlineProximity = CalculateDeadlineProximity(task.Deadline, referenceTime);
        
        var score = w.UrgencyWeight * urgency + w.ImportanceWeight * importance + w.DeadlineWeight * deadlineProximity;
        var explanation = $"U={urgency:F1}×{w.UrgencyWeight} + I={importance:F1}×{w.ImportanceWeight} + D={deadlineProximity:F1}×{w.DeadlineWeight}";
        
        return PriorityScore.Create(score, explanation);
    }
    
    // Higher-order function: create a prioritization function with fixed weights
    public static Func<TaskItem, DateTime, PriorityScore> CreatePrioritizer(PriorityWeights weights) =>
        (task, time) => CalculatePriorityScore(task, time, weights);
    
    // Curried version
    public static Func<DateTime, Func<TaskItem, PriorityScore>> CurriedPrioritize(PriorityWeights? weights = null) =>
        referenceTime => task => CalculatePriorityScore(task, referenceTime, weights);
    
    // Sort tasks by calculated priority
    public static IReadOnlyList<(TaskItem Task, PriorityScore Score)> RankTasks(
        IEnumerable<TaskItem> tasks, DateTime referenceTime, PriorityWeights? weights = null) =>
        tasks.Select(t => (Task: t, Score: CalculatePriorityScore(t, referenceTime, weights)))
             .OrderByDescending(x => x.Score.Value)
             .ToList()
             .AsReadOnly();
}
