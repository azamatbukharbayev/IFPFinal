namespace TaskFlow.Integration.Tests;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Features.Analytics;
using TaskFlow.Application.Features.Projects;
using TaskFlow.Application.Features.Tasks;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Persistence;
using Xunit;

public class EndToEndIntegrationTests
{
    private ServiceProvider CreateTestServiceProvider()
    {
        var services = new ServiceCollection();

        // Use unique in-memory database for each test
        services.AddDbContext<TaskFlowDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: $"TaskFlow_Test_{Guid.NewGuid()}"));

        // Register repositories & services using Infrastructure
        services.AddScoped<TaskFlow.Application.Interfaces.ITaskRepository, TaskFlow.Infrastructure.Repositories.TaskRepository>();
        services.AddScoped<TaskFlow.Application.Interfaces.IProjectRepository, TaskFlow.Infrastructure.Repositories.ProjectRepository>();
        services.AddScoped<TaskFlow.Application.Interfaces.IActivityLogRepository, TaskFlow.Infrastructure.Repositories.ActivityLogRepository>();
        services.AddScoped<TaskFlow.Application.Interfaces.IUnitOfWork, TaskFlow.Infrastructure.Repositories.UnitOfWork>();
        services.AddScoped<TaskFlow.Application.Interfaces.IImportExportService, TaskFlow.Infrastructure.ImportExport.ImportExportService>();

        services.AddScoped<TaskService>();
        services.AddScoped<ProjectService>();
        services.AddScoped<AnalyticsService>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task ProjectAndTaskLifecycle_EndToEnd_WorksCorrectly()
    {
        var sp = CreateTestServiceProvider();
        var projectService = sp.GetRequiredService<ProjectService>();
        var taskService = sp.GetRequiredService<TaskService>();
        var analyticsService = sp.GetRequiredService<AnalyticsService>();

        // 1. Create a Project
        var createProjectResult = await projectService.CreateProjectAsync(new CreateProjectDto(
            "Final Graduation Project",
            "Functional Programming demonstration",
            DateTime.UtcNow.AddDays(14)
        ));
        Assert.True(createProjectResult.IsSuccess);
        var project = (createProjectResult as TaskFlow.Domain.Functional.Success<ProjectDto>)!.Value;

        // 2. Create Tasks for this Project
        var task1Result = await taskService.CreateTaskAsync(new CreateTaskDto(
            "Write Specifications",
            "Define system scope",
            TaskPriority.High,
            DateTime.UtcNow.AddDays(2),
            project.Id,
            4.0m,
            new List<string> { "Docs", "Architecture" },
            null
        ));
        Assert.True(task1Result.IsSuccess);
        var task1 = (task1Result as TaskFlow.Domain.Functional.Success<TaskItemDto>)!.Value;

        var task2Result = await taskService.CreateTaskAsync(new CreateTaskDto(
            "Implement Functional Core",
            "Build pure monads and graph algorithms",
            TaskPriority.Critical,
            DateTime.UtcNow.AddDays(5),
            project.Id,
            8.0m,
            new List<string> { "Core", "FP" },
            null
        ));
        Assert.True(task2Result.IsSuccess);
        var task2 = (task2Result as TaskFlow.Domain.Functional.Success<TaskItemDto>)!.Value;

        // 3. Add Dependency: task2 depends on task1
        var depResult = await taskService.AddDependencyAsync(task2.Id, task1.Id);
        Assert.True(depResult.IsSuccess);

        // 4. Verify task2 cannot be completed before task1
        var completeTask2Early = await taskService.ChangeStatusAsync(task2.Id, TaskItemStatus.Completed);
        Assert.True(completeTask2Early.IsFailure);

        // 5. Complete task1 first
        var completeTask1 = await taskService.ChangeStatusAsync(task1.Id, TaskItemStatus.Completed);
        Assert.True(completeTask1.IsSuccess);

        // 6. Now completing task2 should succeed
        var completeTask2 = await taskService.ChangeStatusAsync(task2.Id, TaskItemStatus.Completed);
        Assert.True(completeTask2.IsSuccess);

        // 7. Verify project progress is updated
        var updatedProject = await projectService.GetProjectByIdAsync(project.Id);
        Assert.NotNull(updatedProject);
        Assert.Equal(100.0, updatedProject.CompletionPercentage);
        Assert.Equal(2, updatedProject.CompletedTaskCount);

        // 8. Verify Dashboard metrics
        var dashboard = await analyticsService.GetDashboardAsync();
        Assert.Equal(2, dashboard.TotalTasks);
        Assert.Equal(2, dashboard.CompletedTasks);
        Assert.Equal(0, dashboard.PendingTasks);
    }
}
