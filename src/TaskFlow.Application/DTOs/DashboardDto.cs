namespace TaskFlow.Application.DTOs;

public record DashboardDto(
    int TotalTasks,
    int CompletedTasks,
    int PendingTasks,
    int InProgressTasks,
    int OverdueTasks,
    int BlockedTasks,
    double CompletionRate,
    double AverageCompletionTimeHours,
    List<TaskItemDto> UpcomingDeadlines,
    List<TaskItemDto> RecentActivity,
    List<ProjectSummaryDto> ProjectSummaries,
    List<WeeklyProductivityDto> WeeklyProductivity);

public record ProjectSummaryDto(Guid Id, string Name, double CompletionPercentage, int TaskCount);
public record WeeklyProductivityDto(string WeekLabel, int CompletedCount);
