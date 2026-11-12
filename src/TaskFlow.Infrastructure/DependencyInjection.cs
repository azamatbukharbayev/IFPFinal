namespace TaskFlow.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Features.Analytics;
using TaskFlow.Application.Features.Projects;
using TaskFlow.Application.Features.Scheduling;
using TaskFlow.Application.Features.Tasks;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.ImportExport;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Repositories;

public static class DependencyInjection
{
    public static IServiceCollection AddTaskFlowInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TaskFlowDbContext>(options =>
            options.UseSqlite(connectionString));
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        services.AddScoped<IImportExportService, ImportExportService>();
        
        // Application services
        services.AddScoped<TaskService>();
        services.AddScoped<ProjectService>();
        services.AddScoped<AnalyticsService>();
        services.AddScoped<SchedulingService>();
        
        return services;
    }
    
    public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TaskFlowDbContext>();
        await context.Database.EnsureCreatedAsync();
        
        if (await context.Projects.AnyAsync())
            return; // Already seeded
        
        await SeedDataAsync(context);
    }
    
    private static async Task SeedDataAsync(TaskFlowDbContext context)
    {
        // Create projects
        var webProject = Project.Create("Website Redesign", "Complete redesign of the corporate website with modern UI/UX", DateTime.UtcNow.AddDays(45));
        var mobileProject = Project.Create("Mobile App MVP", "Develop minimum viable product for the mobile application", DateTime.UtcNow.AddDays(60));
        var apiProject = Project.Create("API Integration", "Integrate third-party APIs for data synchronization", DateTime.UtcNow.AddDays(30));
        
        var projects = new[] { webProject, mobileProject, apiProject };
        foreach (var result in projects)
            result.Match(p => context.Projects.Add(p), _ => { });
        
        await context.SaveChangesAsync();
        
        var projectList = await context.Projects.ToListAsync();
        var webId = projectList[0].Id;
        var mobileId = projectList[1].Id;
        var apiId = projectList[2].Id;
        
        // Create tasks with varied statuses, priorities, and deadlines
        var taskData = new (string Title, string? Desc, TaskPriority Priority, int DaysOffset, TaskItemStatus Status, decimal Effort, Guid? ProjectId, string[]? Labels)[]
        {
            ("Design Homepage Mockup", "Create wireframes and high-fidelity mockups for the homepage", TaskPriority.High, -5, TaskItemStatus.Completed, 8, webId, new[] { "design", "ui" }),
            ("Implement Navigation", "Build responsive navigation component", TaskPriority.High, 3, TaskItemStatus.InProgress, 4, webId, new[] { "frontend" }),
            ("Setup CI/CD Pipeline", "Configure GitHub Actions for automated deployment", TaskPriority.Medium, 7, TaskItemStatus.ToDo, 6, webId, new[] { "devops" }),
            ("Create Contact Form", "Design and implement contact form with validation", TaskPriority.Medium, 10, TaskItemStatus.ToDo, 3, webId, new[] { "frontend", "forms" }),
            ("SEO Optimization", "Implement meta tags, sitemap, and structured data", TaskPriority.Low, 20, TaskItemStatus.ToDo, 5, webId, new[] { "seo" }),
            ("Database Schema Design", "Design the database schema for the mobile app", TaskPriority.Critical, -3, TaskItemStatus.Completed, 10, mobileId, new[] { "backend", "database" }),
            ("User Authentication", "Implement JWT-based authentication", TaskPriority.Critical, 5, TaskItemStatus.InProgress, 12, mobileId, new[] { "backend", "security" }),
            ("Push Notifications", "Integrate push notification service", TaskPriority.High, 15, TaskItemStatus.Blocked, 8, mobileId, new[] { "mobile", "notifications" }),
            ("User Profile Screen", "Build the user profile management screen", TaskPriority.Medium, 12, TaskItemStatus.ToDo, 6, mobileId, new[] { "mobile", "ui" }),
            ("Offline Data Sync", "Implement offline data synchronization", TaskPriority.High, 25, TaskItemStatus.ToDo, 15, mobileId, new[] { "mobile", "sync" }),
            ("App Store Submission", "Prepare and submit app to stores", TaskPriority.Medium, 55, TaskItemStatus.ToDo, 4, mobileId, new[] { "release" }),
            ("Research API Documentation", "Review and document third-party API endpoints", TaskPriority.High, -10, TaskItemStatus.Completed, 4, apiId, new[] { "research" }),
            ("Implement OAuth Flow", "Set up OAuth 2.0 authentication with external services", TaskPriority.Critical, -1, TaskItemStatus.InProgress, 8, apiId, new[] { "auth", "integration" }),
            ("Rate Limiting Handler", "Implement rate limiting and retry logic", TaskPriority.High, 8, TaskItemStatus.ToDo, 6, apiId, new[] { "backend" }),
            ("Data Mapping Layer", "Create DTOs and mapping for external API responses", TaskPriority.Medium, 12, TaskItemStatus.ToDo, 5, apiId, new[] { "backend", "mapping" }),
            ("Error Logging Service", "Centralized error logging for API calls", TaskPriority.Medium, 15, TaskItemStatus.ToDo, 4, apiId, new[] { "monitoring" }),
            ("Performance Testing", "Load test API integration endpoints", TaskPriority.Low, 20, TaskItemStatus.ToDo, 6, apiId, new[] { "testing" }),
            ("Write Unit Tests", "Comprehensive unit test coverage for core logic", TaskPriority.High, -2, TaskItemStatus.InProgress, 10, null, new[] { "testing" }),
            ("Code Review Session", "Review pull requests and provide feedback", TaskPriority.Medium, 1, TaskItemStatus.ToDo, 3, null, new[] { "review" }),
            ("Documentation Update", "Update project documentation and README", TaskPriority.Low, -7, TaskItemStatus.Completed, 2, null, new[] { "docs" }),
            ("Security Audit", "Review code for security vulnerabilities", TaskPriority.Critical, 2, TaskItemStatus.ToDo, 8, null, new[] { "security" }),
            ("Team Standup Notes", "Prepare notes for daily standup meeting", TaskPriority.Low, 0, TaskItemStatus.ToDo, 0.5m, null, new[] { "meeting" }),
        };
        
        var createdTasks = new List<TaskItem>();
        foreach (var (title, desc, priority, daysOffset, status, effort, projectId, labels) in taskData)
        {
            var deadline = DateTime.UtcNow.AddDays(daysOffset);
            var result = TaskItem.Create(title, desc, priority, deadline, projectId, effort, labels?.ToList());
            result.Match(task =>
            {
                if (status != TaskItemStatus.ToDo)
                {
                    if (status == TaskItemStatus.Completed)
                    {
                        task.ChangeStatus(TaskItemStatus.InProgress);
                        task.ChangeStatus(TaskItemStatus.Completed);
                    }
                    else
                    {
                        task.ChangeStatus(status);
                    }
                }
                context.Tasks.Add(task);
                createdTasks.Add(task);
            }, _ => { });
        }
        
        await context.SaveChangesAsync();
        
        // Add some dependencies (valid ones)
        if (createdTasks.Count >= 14)
        {
            // Task 2 (Implement Navigation) depends on Task 1 (Design Homepage)
            context.TaskDependencies.Add(TaskDependency.Create(createdTasks[1].Id, createdTasks[0].Id));
            // Task 4 (Create Contact Form) depends on Task 2
            context.TaskDependencies.Add(TaskDependency.Create(createdTasks[3].Id, createdTasks[1].Id));
            // Task 8 (Push Notifications) depends on Task 7 (User Authentication)
            context.TaskDependencies.Add(TaskDependency.Create(createdTasks[7].Id, createdTasks[6].Id));
            // Task 14 (Rate Limiting) depends on Task 13 (OAuth Flow)
            context.TaskDependencies.Add(TaskDependency.Create(createdTasks[13].Id, createdTasks[12].Id));
        }
        
        // Add activity logs
        context.ActivityLogs.Add(ActivityLog.Create("ProjectCreated", "Project 'Website Redesign' created", projectId: webId));
        context.ActivityLogs.Add(ActivityLog.Create("ProjectCreated", "Project 'Mobile App MVP' created", projectId: mobileId));
        context.ActivityLogs.Add(ActivityLog.Create("ProjectCreated", "Project 'API Integration' created", projectId: apiId));
        
        if (createdTasks.Count > 0)
        {
            context.ActivityLogs.Add(ActivityLog.Create("TaskCompleted", $"Task '{createdTasks[0].Title}' completed", createdTasks[0].Id, webId));
            context.ActivityLogs.Add(ActivityLog.Create("StatusChanged", $"Task '{createdTasks[1].Title}' moved to In Progress", createdTasks[1].Id, webId));
        }
        
        await context.SaveChangesAsync();
    }
}
