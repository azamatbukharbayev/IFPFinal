namespace TaskFlow.Application.DTOs;

using TaskFlow.Domain.Enums;

public record AnalyticsDto(
    int TotalTasks,
    int CompletedTasks,
    int PendingTasks,
    int InProgressTasks,
    int OverdueTasks,
    int BlockedTasks,
    double CompletionRate,
    double AverageCompletionTimeHours,
    double OverdueRatio,
    Dictionary<string, int> PriorityDistribution,
    Dictionary<string, int> StatusDistribution,
    List<WeeklyProductivityDto> WeeklyCompletions,
    List<ProjectProgressDto> ProjectProgress);

public record ProjectProgressDto(Guid ProjectId, string ProjectName, double CompletionPercentage, int TotalTasks, int CompletedTasks);
