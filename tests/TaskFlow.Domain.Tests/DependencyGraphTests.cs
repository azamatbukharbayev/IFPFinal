namespace TaskFlow.Domain.Tests;

using Xunit;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Services;

public class DependencyGraphTests
{
    [Fact]
    public void HasCycle_AcyclicGraph_ReturnsFalse()
    {
        // 1 -> 2 -> 3
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        var deps = new List<TaskDependency>
        {
            TaskDependency.Create(id1, id2),
            TaskDependency.Create(id2, id3)
        };

        var adj = DependencyGraphService.BuildAdjacencyList(deps);
        Assert.False(DependencyGraphService.HasCycle(adj));
    }

    [Fact]
    public void HasCycle_CyclicGraph_ReturnsTrue()
    {
        // 1 -> 2 -> 3 -> 1
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        var deps = new List<TaskDependency>
        {
            TaskDependency.Create(id1, id2),
            TaskDependency.Create(id2, id3),
            TaskDependency.Create(id3, id1)
        };

        var adj = DependencyGraphService.BuildAdjacencyList(deps);
        Assert.True(DependencyGraphService.HasCycle(adj));
    }

    [Fact]
    public void ValidateNewDependency_DetectsCycleBeforeInsertion()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        var existing = new List<TaskDependency>
        {
            TaskDependency.Create(id1, id2)
        };

        // Attempting to add id2 -> id1 creates cycle
        var result = DependencyGraphService.ValidateNewDependency(id2, id1, existing);
        Assert.True(result.IsFailure);
        Assert.Contains("circular dependency", result.Errors[0]);
    }

    [Fact]
    public void ValidateNewDependency_SelfDependency_Fails()
    {
        var id = Guid.NewGuid();
        var result = DependencyGraphService.ValidateNewDependency(id, id, new List<TaskDependency>());
        Assert.True(result.IsFailure);
        Assert.Contains("cannot depend on itself", result.Errors[0]);
    }

    [Fact]
    public void CanCompleteTask_IncompletePrerequisite_ReturnsFailure()
    {
        var task1 = TaskItem.Create("Task 1", null, TaskPriority.High, null, null, 1m, null).GetValueOrDefault(null!);
        var task2 = TaskItem.Create("Task 2", null, TaskPriority.High, null, null, 1m, null).GetValueOrDefault(null!);

        var dep = TaskDependency.Create(task2.Id, task1.Id); // Task 2 depends on Task 1

        var allTasks = new[] { task1, task2 };
        var canComplete = DependencyGraphService.CanCompleteTask(task2.Id, new[] { dep }, allTasks);

        Assert.True(canComplete.IsFailure);
        Assert.Contains("Prerequisite 'Task 1' is not completed", canComplete.Errors[0]);
    }

    [Fact]
    public void CanCompleteTask_CompletedPrerequisite_ReturnsSuccess()
    {
        var task1 = TaskItem.Create("Task 1", null, TaskPriority.High, null, null, 1m, null).GetValueOrDefault(null!);
        task1.ChangeStatus(TaskItemStatus.InProgress);
        task1.ChangeStatus(TaskItemStatus.Completed);

        var task2 = TaskItem.Create("Task 2", null, TaskPriority.High, null, null, 1m, null).GetValueOrDefault(null!);

        var dep = TaskDependency.Create(task2.Id, task1.Id);

        var allTasks = new[] { task1, task2 };
        var canComplete = DependencyGraphService.CanCompleteTask(task2.Id, new[] { dep }, allTasks);

        Assert.True(canComplete.IsSuccess);
    }

    [Fact]
    public void TopologicalSort_ReturnsValidExecutionOrder()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var c = Guid.NewGuid();

        // c depends on b, b depends on a  (a -> b -> c)
        var deps = new List<TaskDependency>
        {
            TaskDependency.Create(c, b),
            TaskDependency.Create(b, a)
        };

        var sortResult = DependencyGraphService.TopologicalSort(new[] { a, b, c }, deps);
        Assert.True(sortResult.IsSuccess);
        var order = sortResult.GetValueOrDefault(new List<Guid>());
        Assert.Equal(3, order.Count);
        var orderList = order.ToList();
        Assert.True(orderList.IndexOf(a) < orderList.IndexOf(b));
        Assert.True(orderList.IndexOf(b) < orderList.IndexOf(c));
    }
}
