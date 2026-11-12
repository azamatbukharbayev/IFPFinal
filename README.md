# TaskFlow — Intelligent Task Management System

[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Blazor Server](https://img.shields.io/badge/Blazor-Server-blue.svg)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![SQLite](https://img.shields.io/badge/Database-SQLite-green.svg)](https://www.sqlite.org/)
[![Tests](https://img.shields.io/badge/Tests-52%20Passed-brightgreen.svg)]()
[![Course](https://img.shields.io/badge/Course-Introduction%20to%20Functional%20Programming-orange.svg)]()

> **University Final Project**  
> **Course:** Introduction to Functional Programming  
> **Institution:** Astana IT University (AITU)  

---

## 1. Project Overview

**TaskFlow** is an intelligent, responsive task management platform engineered with modern C# 13 and .NET 10. It unifies professional task scheduling, Kanban project tracking, and dependency management with a mathematically pure **Functional Core** calculation engine.

Unlike conventional task managers with imperative, mutable business logic, TaskFlow enforces strict mathematical purity, category-theoretic monadic error handling (`Option<T>` and `Result<T>`), DFS cycle detection, Kahn's topological sorting, currying, and stack-safe trampolined recursion.

---

## 2. Team Members & Contributions

| Developer | Role | GitHub Profile | Primary Ownership |
|---|---|---|---|
| **Azamat Bukharbayev** | Lead Backend & Functional Architect | [github.com/azamatbukharbayev](https://github.com/azamatbukharbayev) | Domain Layer, Monadic Engine, Graph Algorithms, Application Services, Persistence (EF Core / SQLite), Automated Domain & Application Tests, Technical Specs. |
| **Ruza Isembekova** | Lead Frontend, UX & QA Specialist | [github.com/ruzaisembekova815-oss](https://github.com/ruzaisembekova815-oss) | Blazor Server Presentation, Responsive Component System, Kanban Board, Calendar View, Toast Notifications, Web & Integration Tests, Architecture Diagrams, Academic Report. |

A full file-by-file ownership breakdown is documented in [OWNERSHIP.md](file:///Users/azamatbukharbayev/Documents/AITU/IFPFinal/OWNERSHIP.md).

---

## 3. Key Functional Highlights & Features

### 3.1. Six Core Business Modules
1. **Executive Dashboard (`Home.razor`):** Dynamic statistics counters (Total, Completed, Pending, In Progress, Overdue, Blocked), effort-weighted project progress meters, weekly productivity charts, upcoming deadlines, and recent activity streams. All values originate from live database queries.
2. **Task Management Center (`TasksPage.razor`):** Full CRUD with client and server-side validation. Multi-criteria filtering (Status, Priority, Project, Search query), multi-mode sorting (Urgency, Deadline, Creation, Priority), subtask management, and grid/card display toggle.
3. **Interactive Kanban Board (`KanbanPage.razor`):** 4-column reactive board (To Do, In Progress, Blocked, Completed) with single-click status transitions and automated dependency checking.
4. **Project Hub (`ProjectsPage.razor`):** Multi-project tracking with effort-weighted completion formulas, deadline warnings, and project lifecycle management.
5. **Interactive Calendar (`CalendarPage.razor`):** Monthly calendar view displaying scheduled deadlines, urgency badges, and quick task details.
6. **Analytics & Performance (`AnalyticsPage.razor`):** Task distribution breakdowns by status and priority, average completion velocity, overdue ratios, and per-project completion comparisons.
7. **Import / Export Center (`SettingsPage.razor`):** RFC-4180 compliant CSV export/import and JSON project synchronization.

### 3.2. Functional Programming Pillars
- **Pure Functions & Referential Transparency:** Deterministic multi-factor priority algorithm $P(t) = w_u \cdot U(t) + w_i \cdot I(t) + w_d \cdot D(t)$.
- **Monads (`Option<T>` & `Result<T>`):** Total elimination of null pointer exceptions and unhandled exception crashes via Railway-Oriented Programming.
- **Currying & Partial Application:** Pre-binding calculation parameters and creating specialized higher-order filter functions.
- **Higher-Order Filter Composition:** Chaining pure query predicates using forward composition ($f \circ g$).
- **Graph Theory & DAG Validation:** Cycle detection via Depth-First Search recursion stack $O(V+E)$ and Kahn's algorithm for topological sorting of task dependencies.
- **Trampolined Stack-Safe Recursion:** Deep recursive effort aggregation with `Bounce<T>` / `Done<T>` / `More<T>` to guarantee zero stack overflows.

---

## 4. Technology Stack & Architecture

- **Runtime & SDK:** .NET 10.0 (C# 13)
- **Frontend Framework:** Blazor Server (Interactive Server Rendering)
- **CSS & UI Design:** Bootstrap 5.3 + Bootstrap Icons + Custom TaskFlow CSS Design System
- **Database & ORM:** SQLite 3 with Entity Framework Core 10 (Code-First)
- **Architecture:** Clean Architecture (Onion) with Functional Core / Imperative Shell
- **Testing Frameworks:** xUnit 2.9, EF Core InMemory, WebApplicationFactory

---

## 5. Prerequisites & Getting Started

### 5.1. Requirements
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or higher.
- macOS, Linux, or Windows 10/11.

### 5.2. Clone and Setup
```bash
# Navigate to the project directory
cd /Users/azamatbukharbayev/Documents/AITU/IFPFinal

# Restore dependencies
dotnet restore

# Build the complete solution
dotnet build --configuration Release
```

### 5.3. Running the Application
```bash
# Launch the Blazor Server application
dotnet run --project src/TaskFlow.Web
```
Once launched, navigate to `http://localhost:5000` or `https://localhost:5001` in your web browser.  
The application automatically creates the SQLite database (`taskflow.db`) and seeds it with 3 sample projects, 22 tasks, 4 dependency relations, and initial activity logs.

---

## 6. Running Automated Tests

TaskFlow contains 52 comprehensive automated tests across all 4 layers:

```bash
# Execute the complete test suite
dotnet test --verbosity normal
```

### Expected Output Summary:
```text
Passed!  - Failed: 0, Passed: 38, Skipped: 0, Total: 38 - TaskFlow.Domain.Tests.dll
Passed!  - Failed: 0, Passed: 10, Skipped: 0, Total: 10 - TaskFlow.Application.Tests.dll
Passed!  - Failed: 0, Passed:  3, Skipped: 0, Total:  3 - TaskFlow.Web.Tests.dll
Passed!  - Failed: 0, Passed:  1, Skipped: 0, Total:  1 - TaskFlow.Integration.Tests.dll

Total Tests Passed: 52 (100% Pass Rate)
```

---

## 7. Manual Git Instructions for Developers

As per academic submission guidelines, all Git repository initialization, staging, and commits are managed manually by the developers.

### Developer A (Azamat Bukharbayev) Staging & Commit
```bash
git add src/TaskFlow.Domain/ \
        src/TaskFlow.Application/ \
        src/TaskFlow.Infrastructure/ \
        tests/TaskFlow.Domain.Tests/ \
        tests/TaskFlow.Application.Tests/ \
        docs/technical/ \
        data/seed/

git commit -m "feat(backend): implement clean architecture, functional core, graph algorithms, and persistence"
```

### Developer B (Ruza Isembekova) Staging & Commit
```bash
git add TaskFlow.sln \
        .gitignore \
        OWNERSHIP.md \
        README.md \
        src/TaskFlow.Web/ \
        tests/TaskFlow.Web.Tests/ \
        tests/TaskFlow.Integration.Tests/ \
        docs/diagrams/ \
        docs/report/ \
        data/import-samples/

git commit -m "feat(frontend): implement blazor server UI, reactive state, test suites, and academic report"
```

---

## 8. Academic Documentation Links
- [Detailed Academic Graduation Report](file:///Users/azamatbukharbayev/Documents/AITU/IFPFinal/docs/report/report.md)
- [Backend Architecture Specification](file:///Users/azamatbukharbayev/Documents/AITU/IFPFinal/docs/technical/backend-architecture.md)
- [Functional Programming Syllabus Mapping](file:///Users/azamatbukharbayev/Documents/AITU/IFPFinal/docs/technical/functional-programming-mapping.md)
- [Functional Programming Code Examples](file:///Users/azamatbukharbayev/Documents/AITU/IFPFinal/docs/technical/functional-examples.md)
- [Backend-Frontend Integration Contract](file:///Users/azamatbukharbayev/Documents/AITU/IFPFinal/docs/technical/integration-contract.md)
