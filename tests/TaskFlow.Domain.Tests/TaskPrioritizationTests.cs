namespace TaskFlow.Domain.Tests;

using Xunit;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Services;

public class TaskPrioritizationTests
{
    [Fact]
    public void CalculateUrgency_OverdueTask_ReturnsMaximumUrgency()
    {
        var now = new DateTime(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);
        var pastDeadline = now.AddDays(-2);

        var urgency = TaskPrioritizationService.CalculateUrgency(pastDeadline, now);
        Assert.Equal(100.0, urgency);
    }

    [Fact]
    public void CalculateUrgency_DueToday_ReturnsHighUrgency()
    {
        var now = new DateTime(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);
        var todayDeadline = now.AddHours(5);

        var urgency = TaskPrioritizationService.CalculateUrgency(todayDeadline, now);
        Assert.Equal(95.0, urgency);
    }

    [Fact]
    public void CalculateUrgency_NoDeadline_ReturnsBaselineUrgency()
    {
        var now = new DateTime(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);
        var urgency = TaskPrioritizationService.CalculateUrgency(null, now);
        Assert.Equal(30.0, urgency);
    }

    [Theory]
    [InlineData(TaskPriority.Critical, 100.0)]
    [InlineData(TaskPriority.High, 75.0)]
    [InlineData(TaskPriority.Medium, 50.0)]
    [InlineData(TaskPriority.Low, 25.0)]
    public void CalculateImportance_MapsCorrectly(TaskPriority priority, double expectedScore)
    {
        var score = TaskPrioritizationService.CalculateImportance(priority);
        Assert.Equal(expectedScore, score);
    }

    [Fact]
    public void CalculatePriorityScore_ProducesDeterministicNormalizedScore()
    {
        var now = new DateTime(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);
        var task = TaskItem.Create("Fix Bug", "Critical issue", TaskPriority.Critical, now.AddHours(4), null, 2m, null).GetValueOrDefault(null!);

        var score = TaskPrioritizationService.CalculatePriorityScore(task, now);

        Assert.NotNull(score);
        Assert.InRange(score.Value, 0.0, 100.0);
        Assert.Contains("U=", score.Explanation);
    }

    [Fact]
    public void RankTasks_OrdersByCalculatedPriorityDescending()
    {
        var now = new DateTime(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);
        var criticalDueSoon = TaskItem.Create("Critical", null, TaskPriority.Critical, now.AddHours(2), null, 1m, null).GetValueOrDefault(null!);
        var lowDueNextMonth = TaskItem.Create("Low", null, TaskPriority.Low, now.AddDays(40), null, 1m, null).GetValueOrDefault(null!);

        var tasks = new[] { lowDueNextMonth, criticalDueSoon };
        var ranked = TaskPrioritizationService.RankTasks(tasks, now);

        Assert.Equal("Critical", ranked[0].Task.Title);
        Assert.Equal("Low", ranked[1].Task.Title);
        Assert.True(ranked[0].Score.Value > ranked[1].Score.Value);
    }
}
