# TaskFlow — Backend Integration Contract

**Author:** Azamat Bukharbayev ([github.com/azamatbukharbayev](https://github.com/azamatbukharbayev))  
**Consumer / Frontend Lead:** Ruza Isembekova ([github.com/ruzaisembekova815-oss](https://github.com/ruzaisembekova815-oss))  
**Course:** Introduction to Functional Programming  
**Institution:** Astana IT University (AITU)  

This document specifies the complete service interface and DTO contract exposed by the Application and Domain layers to the Blazor presentation tier.

---

## 1. Primary Service Contracts

### 1.1. `TaskService`
Located in: `TaskFlow.Application.Features.Tasks`

```csharp
public class TaskService
{
    Task<Result<TaskItemDto>> CreateTaskAsync(CreateTaskDto dto, CancellationToken ct = default);
    Task<Result<TaskItemDto>> UpdateTaskAsync(UpdateTaskDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteTaskAsync(Guid id, CancellationToken ct = default);
    Task<Result<TaskItemDto>> ChangeStatusAsync(Guid taskId, TaskItemStatus newStatus, CancellationToken ct = default);
    Task<IReadOnlyList<TaskItemDto>> GetAllTasksAsync(
        TaskItemStatus? status = null, 
        TaskPriority? priority = null,
        Guid? projectId = null, 
        string? searchTerm = null,
        string? sortBy = null, 
        CancellationToken ct = default);
    Task<TaskItemDto?> GetTaskByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TaskItemDto>> GetSubtasksAsync(Guid parentTaskId, CancellationToken ct = default);
    Task<Result<bool>> AddDependencyAsync(Guid taskId, Guid dependsOnTaskId, CancellationToken ct = default);
    Task<Result<bool>> RemoveDependencyAsync(Guid dependencyId, CancellationToken ct = default);
    Task<IReadOnlyList<(TaskItemDto Task, double Score)>> GetPrioritizedTasksAsync(CancellationToken ct = default);
}
```

### 1.2. `ProjectService`
Located in: `TaskFlow.Application.Features.Projects`

```csharp
public class ProjectService
{
    Task<Result<ProjectDto>> CreateProjectAsync(CreateProjectDto dto, CancellationToken ct = default);
    Task<Result<ProjectDto>> UpdateProjectAsync(UpdateProjectDto dto, CancellationToken ct = default);
    Task<Result<bool>> DeleteProjectAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectDto>> GetAllProjectsAsync(CancellationToken ct = default);
    Task<ProjectDto?> GetProjectByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<bool>> ArchiveProjectAsync(Guid id, CancellationToken ct = default);
}
```

### 1.3. `AnalyticsService`
Located in: `TaskFlow.Application.Features.Analytics`

```csharp
public class AnalyticsService
{
    Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default);
    Task<AnalyticsDto> GetAnalyticsAsync(CancellationToken ct = default);
}
```

### 1.4. `IImportExportService`
Located in: `TaskFlow.Application.Interfaces`

```csharp
public interface IImportExportService
{
    Task<ImportResultDto> ImportTasksFromCsvAsync(Stream csvStream, CancellationToken ct = default);
    Task<byte[]> ExportTasksToCsvAsync(CancellationToken ct = default);
    Task<ImportResultDto> ImportProjectsFromJsonAsync(Stream jsonStream, CancellationToken ct = default);
    Task<byte[]> ExportProjectsToJsonAsync(CancellationToken ct = default);
}
```

---

## 2. Core Data Transfer Objects (DTOs)

### `TaskItemDto`
```csharp
public record TaskItemDto(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime CreatedAt,
    DateTime? Deadline,
    DateTime? CompletedAt,
    Guid? ProjectId,
    string? ProjectName,
    decimal EstimatedEffortHours,
    List<string> Labels,
    Guid? ParentTaskId,
    List<Guid> DependsOnTaskIds,
    double PriorityScore,
    string DeadlineCategory);
```

### `ProjectDto`
```csharp
public record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    ProjectStatus Status,
    DateTime CreatedAt,
    DateTime? Deadline,
    int TaskCount,
    int CompletedTaskCount,
    double CompletionPercentage,
    decimal TotalEffort,
    decimal CompletedEffort);
```

### `DashboardDto`
```csharp
public record DashboardDto(
    int TotalTasks,
    int CompletedTasks,
    int PendingTasks,
    int InProgressTasks,
    int OverdueTasks,
    int BlockedTasks,
    double CompletionRate,
    double AverageCompletionTimeHours,
    IReadOnlyList<TaskItemDto> UpcomingDeadlines,
    IReadOnlyList<TaskItemDto> RecentActivity,
    IReadOnlyList<ProjectSummaryDto> ProjectSummaries,
    IReadOnlyList<WeeklyProductivityDto> WeeklyProductivity);
```

---

## 3. Frontend Error Handling Integration Convention

The Blazor UI unwraps backend `Result<T>` instances cleanly:

```csharp
var result = await TaskService.ChangeStatusAsync(task.Id, newStatus);
result.Match(
    onSuccess: updatedTask => {
        ToastService.ShowSuccess($"Task marked as {newStatus}!");
        StateContainer.NotifyStateChanged();
    },
    onFailure: errors => {
        ToastService.ShowError(string.Join(", ", errors), "Status Change Failed");
    }
);
```
