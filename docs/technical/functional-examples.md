# TaskFlow — Functional Programming Code Walkthrough & Examples

**Author:** Azamat Bukharbayev ([github.com/azamatbukharbayev](https://github.com/azamatbukharbayev))  
**Course:** Introduction to Functional Programming  
**Institution:** Astana IT University (AITU)  

This document showcases practical code implementations of core functional programming paradigms within TaskFlow.

---

## 1. Pure Function & Mathematical Multi-Factor Scoring

In `src/TaskFlow.Domain/Services/TaskPrioritizationService.cs`, task priority scores are calculated through a purely deterministic formula:

$$P(t) = w_u \cdot U(t) + w_i \cdot I(t) + w_d \cdot D(t)$$

Where:
- $U(t)$: Urgency factor based on deadline proximity
- $I(t)$: Importance factor mapped from priority enum
- $D(t)$: Deadline proximity scale $[0, 100]$
- $w_u, w_i, w_d$: Normalization weights summing to $1.0$

```csharp
public static PriorityScore CalculatePriorityScore(
    TaskItem task, 
    DateTime referenceTime, 
    PriorityWeights? weights = null)
{
    var w = weights ?? new PriorityWeights();
    var urgency = CalculateUrgency(task.Deadline, referenceTime);
    var importance = CalculateImportance(task.Priority);
    var deadlineProximity = CalculateDeadlineProximity(task.Deadline, referenceTime);
    
    var score = w.UrgencyWeight * urgency 
              + w.ImportanceWeight * importance 
              + w.DeadlineWeight * deadlineProximity;
              
    var explanation = $"U={urgency:F1}×{w.UrgencyWeight} + I={importance:F1}×{w.ImportanceWeight} + D={deadlineProximity:F1}×{w.DeadlineWeight}";
    
    return PriorityScore.Create(score, explanation);
}
```

---

## 2. Monads: Result<T> and Railway-Oriented Programming

Instead of throwing exceptions, operations return `Result<T>`, chaining subsequent operations safely via `Bind`:

```csharp
// Example Railway: Validate -> Check Dependencies -> Update Status
public async Task<Result<TaskItemDto>> ChangeStatusAsync(Guid taskId, TaskItemStatus newStatus)
{
    var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
    if (task is null)
        return Result<TaskItemDto>.Failure($"Task with ID {taskId} not found.");
    
    // Dependency verification for completion
    if (newStatus == TaskItemStatus.Completed)
    {
        var allDeps = await _unitOfWork.Tasks.GetDependenciesAsync();
        var allTasks = await _unitOfWork.Tasks.GetAllAsync();
        var canComplete = DependencyGraphService.CanCompleteTask(taskId, allDeps, allTasks);
        if (canComplete.IsFailure)
            return Result<TaskItemDto>.Failure(canComplete.Errors);
    }
    
    var result = task.ChangeStatus(newStatus);
    return await result.Match<Task<Result<TaskItemDto>>>(
        async opResult => {
            await _unitOfWork.Tasks.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync();
            return Result<TaskItemDto>.Success(MapToDto(task));
        },
        errors => Task.FromResult(Result<TaskItemDto>.Failure(errors))
    );
}
```

---

## 3. Currying and Partial Application

Currying transforms a multi-parameter function into a sequence of single-parameter functions:

```csharp
// Currying definition in FunctionalExtensions.cs
public static Func<T1, Func<T2, TResult>> Curry<T1, T2, TResult>(this Func<T1, T2, TResult> func) =>
    a => b => func(a, b);

// Practical usage in TaskPrioritizationService.cs
public static Func<DateTime, Func<TaskItem, PriorityScore>> CurriedPrioritize(PriorityWeights? weights = null) =>
    referenceTime => task => CalculatePriorityScore(task, referenceTime, weights);

// Usage: Freeze reference time and prioritize tasks individually
var prioritizeNow = TaskPrioritizationService.CurriedPrioritize()(DateTime.UtcNow);
var score1 = prioritizeNow(task1);
var score2 = prioritizeNow(task2);
```

Partial application fixes specific arguments:

```csharp
// Partial application helper in TaskCollectionExtensions.cs
public static Func<IEnumerable<TaskItem>, IEnumerable<TaskItem>> FilterByStatus(TaskItemStatus status) =>
    tasks => tasks.Where(t => t.Status == status);

var getInProgress = TaskCollectionExtensions.FilterByStatus(TaskItemStatus.InProgress);
var inProgressTasks = getInProgress(allTasks);
```

---

## 4. Higher-Order Filter Pipeline (Function Composition)

Dynamic filtering in TaskFlow is built by chaining pure filter functions using forward composition (`Then`):

```csharp
public static Func<IEnumerable<TaskItem>, IEnumerable<TaskItem>> CreateFilterPipeline(
    TaskItemStatus? status = null,
    TaskPriority? priority = null,
    Guid? projectId = null,
    string? searchTerm = null)
{
    Func<IEnumerable<TaskItem>, IEnumerable<TaskItem>> pipeline = tasks => tasks;
    
    if (status.HasValue)
    {
        var s = status.Value;
        pipeline = pipeline.Then(tasks => tasks.Where(t => t.Status == s));
    }
    if (priority.HasValue)
    {
        var p = priority.Value;
        pipeline = pipeline.Then(tasks => tasks.Where(t => t.Priority == p));
    }
    if (projectId.HasValue)
    {
        var pid = projectId.Value;
        pipeline = pipeline.Then(tasks => tasks.Where(t => t.ProjectId == pid));
    }
    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        var term = searchTerm.ToLowerInvariant();
        pipeline = pipeline.Then(tasks => tasks.Where(t => 
            t.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            (t.Description?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)));
    }
    
    return pipeline;
}
```

---

## 5. Trampolining for Stack-Safe Recursion

To prevent stack overflow exceptions when calculating effort across arbitrarily deep subtask hierarchies:

```csharp
public abstract record Bounce<T>;
public sealed record Done<T>(T Value) : Bounce<T>;
public sealed record More<T>(Func<Bounce<T>> Next) : Bounce<T>;

public static T Trampoline<T>(Func<Bounce<T>> thunk)
{
    var result = thunk();
    while (result is More<T> more)
        result = more.Next();
    return ((Done<T>)result).Value;
}

// In ProjectProgressService.cs
public static decimal CalculateTotalEffortTrampoline(
    TaskItem task, 
    IReadOnlyDictionary<Guid, IReadOnlyList<TaskItem>> childrenMap)
{
    return FunctionalExtensions.Trampoline(() => EffortBounce(task, childrenMap, 0m));
}
```
