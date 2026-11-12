# TaskFlow — Backend Architecture Documentation

**Author:** Azamat Bukharbayev ([github.com/azamatbukharbayev](https://github.com/azamatbukharbayev))  
**Course:** Introduction to Functional Programming  
**Institution:** Astana IT University (AITU)  

---

## 1. Architectural Philosophy

TaskFlow's backend is designed using the **Functional Core, Imperative Shell** paradigm, structured through **Clean Architecture (Onion Architecture)** layers.

```
       +-------------------------------------------------------+
       |                  Presentation (Web)                   |
       |  +-------------------------------------------------+  |
       |  |               Infrastructure                    |  |
       |  |  +-------------------------------------------+  |  |
       |  |  |            Application Layer              |  |  |
       |  |  |  +-------------------------------------+  |  |  |
       |  |  |  |             Domain Core             |  |  |  |
       |  |  |  |       (Pure Functional Engine)      |  |  |  |
       |  |  |  +-------------------------------------+  |  |  |
       |  |  +-------------------------------------------+  |  |
       |  +-------------------------------------------------+  |
       +-------------------------------------------------------+
```

### The Functional Core / Imperative Shell Boundary
1. **The Functional Core (`TaskFlow.Domain`)**:
   - Zero side-effects: no network requests, no database operations, no file I/O, no mutable global state.
   - All domain calculations, graph analyses, validation rules, and algorithms are pure mathematical functions: $f(x) \rightarrow y$.
   - Immutability enforced using C# records, read-only collections (`IReadOnlyList<T>`, `IReadOnlyDictionary<K,V>`), and private setters.
   - Expresses failure through monadic types: `Option<T>` and `Result<T>` instead of throwing runtime exceptions.

2. **The Imperative Shell (`TaskFlow.Application` & `TaskFlow.Infrastructure`)**:
   - Handles impure boundaries: fetching data from SQLite, writing audit logs, timestamp generation (`DateTime.UtcNow`), and file persistence.
   - Sequences pure transformations by unwrapping domain monads via `.Match()`, `.Bind()`, and `.Map()`.

---

## 2. Layer Structure and Dependencies

### 2.1. Domain Layer (`src/TaskFlow.Domain`)
- **No external dependencies** (no NuGet packages, pure C# / BCL only).
- **Core Entities:** `TaskItem`, `Project`, `TaskDependency`, `ActivityLog`.
- **Value Objects:** `PriorityScore`, `Effort`, `Label`, `DateRange`.
- **Monadic Primitives:** `Option<T>`, `Result<T>`, `ValidationResult`, `TaskOperationResult`.
- **Domain Services:**
  - `TaskPrioritizationService`: Mathematical multi-factor ranking.
  - `DependencyGraphService`: Directed Acyclic Graph (DAG) cycle validation and Kahn's topological sort.
  - `ProjectProgressService`: Recursive and trampolined effort-weighted progress aggregation.
  - `TaskAnalyticsService`: Higher-order filter pipelines and statistical aggregations.
  - `TaskCollectionExtensions`: Curried filtering, LINQ query builders, and grouping combinators.

### 2.2. Application Layer (`src/TaskFlow.Application`)
- **Depends only on Domain.**
- Defines Data Transfer Objects (DTOs) for boundary isolation.
- Declares repository interfaces: `ITaskRepository`, `IProjectRepository`, `IActivityLogRepository`, `IUnitOfWork`, `IImportExportService`.
- Coordinates use cases:
  - `TaskService`: Full task lifecycle management with dependency checks.
  - `ProjectService`: Project progress tracking and archiving.
  - `AnalyticsService`: Dashboard and deep metrics computation.
  - `SchedulingService`: Monthly date range aggregation.

### 2.3. Infrastructure Layer (`src/TaskFlow.Infrastructure`)
- **Depends on Application and Domain.**
- Implements data access using **Entity Framework Core 10** with **SQLite**.
- Implements Unit of Work pattern for ACID transaction management.
- Implements custom RFC-4180 CSV parser and JSON data import/export engine.
- Contains automated database seeding with realistic university and engineering tasks.

---

## 3. Dependency Injection Configuration

All services are wired up via extension methods in `src/TaskFlow.Infrastructure/DependencyInjection.cs`:

```csharp
public static IServiceCollection AddTaskFlowInfrastructure(
    this IServiceCollection services, 
    string connectionString)
{
    services.AddDbContext<TaskFlowDbContext>(options =>
        options.UseSqlite(connectionString));
    
    // Repositories & Unit of Work
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    services.AddScoped<ITaskRepository, TaskRepository>();
    services.AddScoped<IProjectRepository, ProjectRepository>();
    services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
    services.AddScoped<IImportExportService, ImportExportService>();
    
    // Application Use Cases
    services.AddScoped<TaskService>();
    services.AddScoped<ProjectService>();
    services.AddScoped<AnalyticsService>();
    services.AddScoped<SchedulingService>();
    
    return services;
}
```

---

## 4. Error Handling and Monadic Flow

TaskFlow eliminates silent null reference exceptions and unhandled exception crashes by replacing exceptions with `Result<T>` and `Option<T>` monads.

When creating a task:
```csharp
var result = TaskItem.Create(title, description, priority, deadline, projectId, effort, labels);

return await result.Match<Task<Result<TaskItemDto>>>(
    async task => {
        await _unitOfWork.Tasks.AddAsync(task);
        await _unitOfWork.SaveChangesAsync();
        return Result<TaskItemDto>.Success(MapToDto(task));
    },
    errors => Task.FromResult(Result<TaskItemDto>.Failure(errors))
);
```

This guarantees compile-time enforcement of error handling throughout the application stack.
