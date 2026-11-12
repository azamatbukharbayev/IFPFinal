namespace TaskFlow.Domain.Tests;

using Xunit;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Services;

public class ProjectProgressTests
{
    [Fact]
    public void CalculateProgress_EmptyProject_ReturnsZero()
    {
        var report = ProjectProgressService.CalculateProgress(new List<TaskItem>());
        Assert.Equal(0, report.CompletionPercentage);
        Assert.Equal(0, report.TotalTasks);
        Assert.Equal(0, report.CompletedTasks);
        Assert.Equal("empty", report.Method);
    }

    [Fact]
    public void CalculateProgress_EffortWeighted_CalculatesCorrectPercentage()
    {
        var t1 = TaskItem.Create("Task 1", null, TaskPriority.Medium, null, null, 10m, null).GetValueOrDefault(null!);
        t1.ChangeStatus(TaskItemStatus.InProgress);
        t1.ChangeStatus(TaskItemStatus.Completed); // 10h done

        var t2 = TaskItem.Create("Task 2", null, TaskPriority.Medium, null, null, 30m, null).GetValueOrDefault(null!); // 30h pending

        var report = ProjectProgressService.CalculateProgress(new[] { t1, t2 });
        // Completed: 10 / 40 = 25%
        Assert.Equal(25.0, report.CompletionPercentage);
        Assert.Equal(2, report.TotalTasks);
        Assert.Equal(1, report.CompletedTasks);
        Assert.Equal(40m, report.TotalEffort);
        Assert.Equal(10m, report.CompletedEffort);
        Assert.Equal("effort-weighted", report.Method);
    }

    [Fact]
    public void CalculateProgress_ZeroEffortSpecified_FallsBackToCountBased()
    {
        var t1 = TaskItem.Create("Task 1", null, TaskPriority.Medium, null, null, 0m, null).GetValueOrDefault(null!);
        t1.ChangeStatus(TaskItemStatus.InProgress);
        t1.ChangeStatus(TaskItemStatus.Completed);

        var t2 = TaskItem.Create("Task 2", null, TaskPriority.Medium, null, null, 0m, null).GetValueOrDefault(null!);

        var report = ProjectProgressService.CalculateProgress(new[] { t1, t2 });
        Assert.Equal(50.0, report.CompletionPercentage);
        Assert.Equal("count-based", report.Method);
    }

    [Fact]
    public void CalculateTotalEffortRecursive_AggregatesParentAndChildren()
    {
        var parent = TaskItem.Create("Parent", null, TaskPriority.High, null, null, 5m, null).GetValueOrDefault(null!);
        var child1 = TaskItem.Create("Child 1", null, TaskPriority.Medium, null, null, 3m, null, parent.Id).GetValueOrDefault(null!);
        var child2 = TaskItem.Create("Child 2", null, TaskPriority.Medium, null, null, 2m, null, parent.Id).GetValueOrDefault(null!);

        var childrenMap = new Dictionary<Guid, IReadOnlyList<TaskItem>>
        {
            [parent.Id] = new[] { child1, child2 }
        };

        var total = ProjectProgressService.CalculateTotalEffortRecursive(parent, childrenMap);
        Assert.Equal(10m, total); // 5 + 3 + 2 = 10
    }

    [Fact]
    public void ValidateStatusTransition_ValidAndInvalidTransitions_FollowBusinessRules()
    {
        // ToDo -> InProgress: valid
        var valid = TaskItem.ValidateStatusTransition(TaskItemStatus.ToDo, TaskItemStatus.InProgress);
        Assert.True(valid.IsSuccess);

        // Completed -> Blocked: invalid (must go back to ToDo first)
        var invalid = TaskItem.ValidateStatusTransition(TaskItemStatus.Completed, TaskItemStatus.Blocked);
        Assert.True(invalid.IsFailure);

        // Same status -> invalid
        var same = TaskItem.ValidateStatusTransition(TaskItemStatus.ToDo, TaskItemStatus.ToDo);
        Assert.True(same.IsFailure);
    }
}
