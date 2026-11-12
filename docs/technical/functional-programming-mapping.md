# TaskFlow — Functional Programming Syllabus Mapping

**Author:** Azamat Bukharbayev ([github.com/azamatbukharbayev](https://github.com/azamatbukharbayev))  
**Course:** Introduction to Functional Programming  
**Institution:** Astana IT University (AITU)  

This document provides a systematic mapping between the core topics of the **Introduction to Functional Programming** curriculum and their direct implementation in TaskFlow's codebase.

---

## 1. Syllabus Concepts Implementation Matrix

| No. | Functional Programming Concept | Implementation Location in TaskFlow | Description & Role in System |
|---|---|---|---|
| **1** | **Pure Functions & Referential Transparency** | `TaskPrioritizationService.cs`, `DependencyGraphService.cs`, `ProjectProgressService.cs` | Calculation of priority scores $P(t)$, graph cycle detection, and progress calculation yield identical outputs for identical inputs with zero side-effects. |
| **2** | **Immutability & Persistent Structures** | `PriorityScore.cs`, `Effort.cs`, `Label.cs`, `Option.cs`, `Result.cs`, `TaskOperationResult.cs` | Domain models, Value Objects, and monads are modeled as immutable C# `record` types and return read-only interfaces (`IReadOnlyList<T>`). |
| **3** | **Monads: Option / Maybe Monad** | `src/TaskFlow.Domain/Functional/Option.cs` | Encapsulates optional values without null. Supports `Map`, `Bind`, `Match`, `Where`, and `ToOption` extension methods. |
| **4** | **Monads: Result / Either Monad** | `src/TaskFlow.Domain/Functional/Result.cs` | Models operations that can fail without throwing exceptions. Accumulates error lists and supports `Map`, `Bind`, `Combine`, and pattern matching. |
| **5** | **Currying** | `src/TaskFlow.Domain/Functional/FunctionalExtensions.cs#L6-L10`, `TaskPrioritizationService.cs#L67` | Converts multi-argument functions into unary function chains: `Func<T1, Func<T2, TResult>>`. Used to pre-bind prioritization weights. |
| **6** | **Partial Application** | `FunctionalExtensions.cs#L13-L17`, `TaskCollectionExtensions.cs#L40-L44` | Fixes a subset of parameters, creating reusable specialized filtering functions: `FilterByStatus(TaskItemStatus.InProgress)`. |
| **7** | **Function Composition (Pipelines)** | `FunctionalExtensions.cs#L20-L24`, `TaskAnalyticsService.cs#L88-L126` | Implements forward composition (`Then`: $f \circ g$) and `Pipe` to chain dynamic query filters sequentially. |
| **8** | **Higher-Order Functions (HOFs)** | `TaskPrioritizationService.cs#L63`, `TaskAnalyticsService.cs#L88`, `Option.cs`, `Result.cs` | Functions taking functions as arguments or returning new functions (e.g. `CreatePrioritizer`, `CreateFilterPipeline`, `Map`, `Bind`). |
| **9** | **Pattern Matching & Discriminated Unions** | `TaskOperationResult.cs`, `TaskItem.cs#L137-L151`, `TaskAnalyticsService.cs#L75-L85` | Type-safe discriminated union records (`Created`, `Updated`, `StatusChanged`) pattern-matched via C# `switch` expressions. |
| **10** | **Recursion** | `src/TaskFlow.Domain/Services/ProjectProgressService.cs#L42-L49` | Traverses nested subtask trees to recursively aggregate total estimated effort hours across arbitrary hierarchies. |
| **11** | **Trampolining (Stack-Safe Recursion)** | `FunctionalExtensions.cs#L45-L56`, `ProjectProgressService.cs#L52-L66` | Prevents call stack overflows for deeply nested task hierarchies using `Bounce<T>`, `Done<T>`, and `More<T>` thunks. |
| **12** | **Memoization** | `src/TaskFlow.Domain/Functional/FunctionalExtensions.cs#L30-L42` | Pure function caching wrapper storing previously evaluated arguments in an internal dictionary to optimize repeated calculations. |
| **13** | **LINQ as Monadic List Processing** | `TaskCollectionExtensions.cs`, `DependencyGraphService.cs`, `AnalyticsService.cs` | Declarative transformations (`Select`, `Where`, `GroupBy`, `Sum`, `OrderByDescending`) applied across task collections. |
| **14** | **Graph Algorithms (Kahn's & DFS)** | `src/TaskFlow.Domain/Services/DependencyGraphService.cs` | Pure cycle detection using Depth-First Search with recursion stack $O(V+E)$ and topological ordering using Kahn's algorithm. |
| **15** | **Side-Effect Isolation (Tap)** | `src/TaskFlow.Domain/Functional/FunctionalExtensions.cs#L27` | Executes audited diagnostic side-effects along a pipeline while cleanly returning the original unchanged immutable value. |

---

## 2. Monadic Laws Verification

Both `Option<T>` and `Result<T>` adhere strictly to mathematical category theory Monadic Laws:

1. **Left Identity:**
   ```csharp
   // Option
   Option<int>.Some(x).Bind(f) == f(x)
   // Result
   Result<int>.Success(x).Bind(f) == f(x)
   ```
2. **Right Identity:**
   ```csharp
   // Option
   m.Bind(x => Option<int>.Some(x)) == m
   // Result
   m.Bind(x => Result<int>.Success(x)) == m
   ```
3. **Associativity:**
   ```csharp
   // Option
   m.Bind(f).Bind(g) == m.Bind(x => f(x).Bind(g))
   // Result
   m.Bind(f).Bind(g) == m.Bind(x => f(x).Bind(g))
   ```

These invariants are covered by automated unit tests in `tests/TaskFlow.Domain.Tests/FunctionalMonadsTests.cs`.
