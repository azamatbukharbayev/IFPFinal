namespace TaskFlow.Application.Tests;

using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Features.Analytics;
using TaskFlow.Application.Features.Projects;
using TaskFlow.Application.Features.Tasks;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Repositories;
using Xunit;

public class ApplicationServiceTests : IDisposable
{
    private readonly TaskFlowDbContext _context;
    private readonly UnitOfWork _uow;
    private readonly TaskService _taskService;
    private readonly ProjectService _projectService;
    private readonly AnalyticsService _analyticsService;

    public ApplicationServiceTests()
    {
        var options = new DbContextOptionsBuilder<TaskFlowDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TaskFlowDbContext(options);
        _uow = new UnitOfWork(_context);
        _taskService = new TaskService(_uow);
        _projectService = new ProjectService(_uow);
        _analyticsService = new AnalyticsService(_uow);
    }

    public void Dispose()
    {
        _uow.Dispose();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateTask_ValidDto_PersistsAndReturnsDto()
    {
        var dto = new CreateTaskDto("Build Feature X", "Details", TaskPriority.High, DateTime.UtcNow.AddDays(5), null, 8m, new List<string> { "backend" }, null);
        var result = await _taskService.CreateTaskAsync(dto);

        Assert.True(result.IsSuccess);
        var created = result.GetValueOrDefault(null!);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("Build Feature X", created.Title);
        Assert.Equal(TaskItemStatus.ToDo, created.Status);

        var retrieved = await _taskService.GetTaskByIdAsync(created.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Build Feature X", retrieved.Title);
    }

    [Fact]
    public async Task CreateTask_InvalidDto_ReturnsValidationFailure()
    {
        var dto = new CreateTaskDto("", "Details", TaskPriority.High, null, null, -5m, null, null);
        var result = await _taskService.CreateTaskAsync(dto);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Contains("Title is required"));
        Assert.Contains(result.Errors, e => e.Contains("cannot be negative"));
    }

    [Fact]
    public async Task ChangeStatus_UpdatesStatusAndCompletedTimestamp()
    {
        var create = await _taskService.CreateTaskAsync(new CreateTaskDto("Task", null, TaskPriority.Medium, null, null, 2m, null, null));
        var taskId = create.GetValueOrDefault(null!).Id;

        // Move to InProgress
        var toInProgress = await _taskService.ChangeStatusAsync(taskId, TaskItemStatus.InProgress);
        Assert.True(toInProgress.IsSuccess);
        Assert.Equal(TaskItemStatus.InProgress, toInProgress.GetValueOrDefault(null!).Status);

        // Move to Completed
        var toCompleted = await _taskService.ChangeStatusAsync(taskId, TaskItemStatus.Completed);
        Assert.True(toCompleted.IsSuccess);
        var completedDto = toCompleted.GetValueOrDefault(null!);
        Assert.Equal(TaskItemStatus.Completed, completedDto.Status);
        Assert.NotNull(completedDto.CompletedAt);
    }

    [Fact]
    public async Task ChangeStatus_CannotCompleteWithUnfinishedPrerequisites()
    {
        var t1 = (await _taskService.CreateTaskAsync(new CreateTaskDto("Prereq", null, TaskPriority.High, null, null, 1m, null, null))).GetValueOrDefault(null!);
        var t2 = (await _taskService.CreateTaskAsync(new CreateTaskDto("Dependent", null, TaskPriority.High, null, null, 1m, null, null))).GetValueOrDefault(null!);

        // Add dependency: t2 depends on t1
        var depResult = await _taskService.AddDependencyAsync(t2.Id, t1.Id);
        Assert.True(depResult.IsSuccess);

        // Try to complete t2 directly
        await _taskService.ChangeStatusAsync(t2.Id, TaskItemStatus.InProgress);
        var completeResult = await _taskService.ChangeStatusAsync(t2.Id, TaskItemStatus.Completed);

        Assert.True(completeResult.IsFailure);
        Assert.Contains(completeResult.Errors, e => e.Contains("Prerequisite 'Prereq' is not completed"));
    }

    [Fact]
    public async Task ProjectCrud_And_ProgressCalculation()
    {
        var projResult = await _projectService.CreateProjectAsync(new CreateProjectDto("Core Platform", "Platform revamp", DateTime.UtcNow.AddMonths(1)));
        Assert.True(projResult.IsSuccess);
        var project = projResult.GetValueOrDefault(null!);

        // Add 2 tasks: one 4h, one 12h (total 16h)
        var t1 = (await _taskService.CreateTaskAsync(new CreateTaskDto("Task 1", null, TaskPriority.Medium, null, project.Id, 4m, null, null))).GetValueOrDefault(null!);
        var t2 = (await _taskService.CreateTaskAsync(new CreateTaskDto("Task 2", null, TaskPriority.Medium, null, project.Id, 12m, null, null))).GetValueOrDefault(null!);

        // Complete Task 1 (4h of 16h = 25%)
        await _taskService.ChangeStatusAsync(t1.Id, TaskItemStatus.InProgress);
        await _taskService.ChangeStatusAsync(t1.Id, TaskItemStatus.Completed);

        var updatedProject = await _projectService.GetProjectByIdAsync(project.Id);
        Assert.NotNull(updatedProject);
        Assert.Equal(2, updatedProject.TaskCount);
        Assert.Equal(1, updatedProject.CompletedTaskCount);
        Assert.Equal(25.0, updatedProject.CompletionPercentage);
        Assert.Equal(16m, updatedProject.TotalEffort);
        Assert.Equal(4m, updatedProject.CompletedEffort);
    }

    [Fact]
    public async Task Analytics_ReturnsValidSnapshotFromRealData()
    {
        await _taskService.CreateTaskAsync(new CreateTaskDto("Task A", null, TaskPriority.High, DateTime.UtcNow.AddDays(1), null, 5m, null, null));
        var tB = (await _taskService.CreateTaskAsync(new CreateTaskDto("Task B", null, TaskPriority.Critical, DateTime.UtcNow.AddDays(-1), null, 3m, null, null))).GetValueOrDefault(null!);

        await _taskService.ChangeStatusAsync(tB.Id, TaskItemStatus.InProgress);
        await _taskService.ChangeStatusAsync(tB.Id, TaskItemStatus.Completed);

        var analytics = await _analyticsService.GetAnalyticsAsync();

        Assert.Equal(2, analytics.TotalTasks);
        Assert.Equal(1, analytics.CompletedTasks);
        Assert.Equal(50.0, analytics.CompletionRate);
    }
}
