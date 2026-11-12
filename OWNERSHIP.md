# TaskFlow — File Ownership & Team Allocation Matrix

**Course:** Introduction to Functional Programming  
**Institution:** Astana IT University (AITU)  
**Project:** TaskFlow — Intelligent Task Management System  

---

## 1. Team Profiles

| Role | Developer Name | GitHub Profile | Primary Responsibilities |
|---|---|---|---|
| **Developer A** | **Azamat Bukharbayev** | [github.com/azamatbukharbayev](https://github.com/azamatbukharbayev) | Domain Architecture, Functional Core, Monadic Primitives, Graph Algorithms, Calculation Engine, Application Services, Persistence (EF Core / SQLite), Repository Pattern, Domain & Application Tests, Technical Architecture Specs. |
| **Developer B** | **Ruza Isembekova** | [github.com/ruzaisembekova815-oss](https://github.com/ruzaisembekova815-oss) | Presentation Architecture, Blazor Server UI/UX, Component Tree, Reactive State Container, Kanban Board, Calendar View, Toast Notifications, Web & End-to-End Integration Tests, Mermaid Architecture Diagrams, Final Academic Report, Solution & Root Configs. |

---

## 2. File Ownership Table

### Developer A — Azamat Bukharbayev (Backend / Functional Core / Persistence)

| Directory / File Path | Purpose & Content | Ownership |
|---|---|---|
| `src/TaskFlow.Domain/TaskFlow.Domain.csproj` | Domain Project Configuration | Azamat Bukharbayev |
| `src/TaskFlow.Domain/Enums/TaskItemStatus.cs` | Task state definitions (ToDo, InProgress, Blocked, Completed) | Azamat Bukharbayev |
| `src/TaskFlow.Domain/Enums/TaskPriority.cs` | Priority levels (Low, Medium, High, Critical) | Azamat Bukharbayev |
| `src/TaskFlow.Domain/Enums/ProjectStatus.cs` | Project lifecycle status (Active, Completed, Archived) | Azamat Bukharbayev |
| `src/TaskFlow.Domain/ValueObjects/PriorityScore.cs` | Immutable value object for composite prioritization scores | Azamat Bukharbayev |
| `src/TaskFlow.Domain/ValueObjects/Effort.cs` | Immutable effort measurement value object | Azamat Bukharbayev |
| `src/TaskFlow.Domain/ValueObjects/Label.cs` | Tagging and categorization value object | Azamat Bukharbayev |
| `src/TaskFlow.Domain/ValueObjects/DateRange.cs` | Scheduling interval value object | Azamat Bukharbayev |
| `src/TaskFlow.Domain/Functional/Option.cs` | Option<T> monad (Some/None) with Map, Bind, Match, Where | Azamat Bukharbayev |
| `src/TaskFlow.Domain/Functional/Result.cs` | Result<T> monad (Success/Failure) with Map, Bind, Combine | Azamat Bukharbayev |
| `src/TaskFlow.Domain/Functional/FunctionalExtensions.cs` | Currying, partial application, composition, trampolining, memoization | Azamat Bukharbayev |
| `src/TaskFlow.Domain/Functional/ValidationResult.cs` | Discriminated union for pure functional domain validation | Azamat Bukharbayev |
| `src/TaskFlow.Domain/Functional/TaskOperationResult.cs` | Discriminated union for domain operation audit trails | Azamat Bukharbayev |
| `src/TaskFlow.Domain/Entities/TaskItem.cs` | Core aggregate root with pure validation and state transitions | Azamat Bukharbayev |
| `src/TaskFlow.Domain/Entities/Project.cs` | Project aggregate entity | Azamat Bukharbayev |
| `src/TaskFlow.Domain/Entities/TaskDependency.cs` | Directed edge entity in dependency graph | Azamat Bukharbayev |
| `src/TaskFlow.Domain/Entities/ActivityLog.cs` | Domain event audit log entity | Azamat Bukharbayev |
| `src/TaskFlow.Domain/Services/TaskPrioritizationService.cs` | Pure scoring formula: P(t) = wu*U(t) + wi*I(t) + wd*D(t) | Azamat Bukharbayev |
| `src/TaskFlow.Domain/Services/DependencyGraphService.cs` | DFS cycle detection O(V+E) and Kahn's topological sort | Azamat Bukharbayev |
| `src/TaskFlow.Domain/Services/ProjectProgressService.cs` | Effort-weighted recursive and trampolined progress calculator | Azamat Bukharbayev |
| `src/TaskFlow.Domain/Services/TaskAnalyticsService.cs` | Higher-order filter pipeline builder and statistical analyzer | Azamat Bukharbayev |
| `src/TaskFlow.Domain/Services/TaskCollectionExtensions.cs` | LINQ and functional collection extension methods | Azamat Bukharbayev |
| `src/TaskFlow.Application/TaskFlow.Application.csproj` | Application Project Configuration | Azamat Bukharbayev |
| `src/TaskFlow.Application/DTOs/TaskItemDto.cs` | Task data transfer record | Azamat Bukharbayev |
| `src/TaskFlow.Application/DTOs/CreateTaskDto.cs` | Task creation command payload | Azamat Bukharbayev |
| `src/TaskFlow.Application/DTOs/UpdateTaskDto.cs` | Task update command payload | Azamat Bukharbayev |
| `src/TaskFlow.Application/DTOs/ProjectDto.cs` | Project data transfer record | Azamat Bukharbayev |
| `src/TaskFlow.Application/DTOs/CreateProjectDto.cs` | Project creation command payload | Azamat Bukharbayev |
| `src/TaskFlow.Application/DTOs/UpdateProjectDto.cs` | Project update command payload | Azamat Bukharbayev |
| `src/TaskFlow.Application/DTOs/DashboardDto.cs` | Aggregated dashboard presentation model | Azamat Bukharbayev |
| `src/TaskFlow.Application/DTOs/AnalyticsDto.cs` | Deep statistical metrics presentation model | Azamat Bukharbayev |
| `src/TaskFlow.Application/DTOs/ImportExportDto.cs` | CSV and JSON serialization structures | Azamat Bukharbayev |
| `src/TaskFlow.Application/Interfaces/ITaskRepository.cs` | Task persistence contract | Azamat Bukharbayev |
| `src/TaskFlow.Application/Interfaces/IProjectRepository.cs` | Project persistence contract | Azamat Bukharbayev |
| `src/TaskFlow.Application/Interfaces/IActivityLogRepository.cs` | Activity log persistence contract | Azamat Bukharbayev |
| `src/TaskFlow.Application/Interfaces/IUnitOfWork.cs` | Unit of work transactional coordinator | Azamat Bukharbayev |
| `src/TaskFlow.Application/Interfaces/IImportExportService.cs` | Data interchange service contract | Azamat Bukharbayev |
| `src/TaskFlow.Application/Features/Tasks/TaskService.cs` | Task orchestration and functional workflow service | Azamat Bukharbayev |
| `src/TaskFlow.Application/Features/Projects/ProjectService.cs` | Project lifecycle and effort aggregation service | Azamat Bukharbayev |
| `src/TaskFlow.Application/Features/Analytics/AnalyticsService.cs` | Real-time analytics and KPI calculation coordinator | Azamat Bukharbayev |
| `src/TaskFlow.Application/Features/Scheduling/SchedulingService.cs` | Calendar view query and interval generator | Azamat Bukharbayev |
| `src/TaskFlow.Infrastructure/TaskFlow.Infrastructure.csproj` | Infrastructure Project Configuration | Azamat Bukharbayev |
| `src/TaskFlow.Infrastructure/Persistence/TaskFlowDbContext.cs` | EF Core SQLite DbContext | Azamat Bukharbayev |
| `src/TaskFlow.Infrastructure/Configurations/TaskItemConfiguration.cs` | EF Core fluent mapping for TaskItem | Azamat Bukharbayev |
| `src/TaskFlow.Infrastructure/Configurations/ProjectConfiguration.cs` | EF Core fluent mapping for Project | Azamat Bukharbayev |
| `src/TaskFlow.Infrastructure/Configurations/TaskDependencyConfiguration.cs` | EF Core fluent mapping for TaskDependency | Azamat Bukharbayev |
| `src/TaskFlow.Infrastructure/Configurations/ActivityLogConfiguration.cs` | EF Core fluent mapping for ActivityLog | Azamat Bukharbayev |
| `src/TaskFlow.Infrastructure/Repositories/TaskRepository.cs` | SQLite task data access implementation | Azamat Bukharbayev |
| `src/TaskFlow.Infrastructure/Repositories/ProjectRepository.cs` | SQLite project data access implementation | Azamat Bukharbayev |
| `src/TaskFlow.Infrastructure/Repositories/ActivityLogRepository.cs` | SQLite audit log access implementation | Azamat Bukharbayev |
| `src/TaskFlow.Infrastructure/Repositories/UnitOfWork.cs` | SQLite unit of work transaction wrapper | Azamat Bukharbayev |
| `src/TaskFlow.Infrastructure/ImportExport/ImportExportService.cs` | Robust custom CSV RFC-4180 parser & JSON exporter | Azamat Bukharbayev |
| `src/TaskFlow.Infrastructure/DependencyInjection.cs` | Service registrations & database seeder with realistic tasks | Azamat Bukharbayev |
| `tests/TaskFlow.Domain.Tests/FunctionalMonadsTests.cs` | Tests for Option, Result, and monadic laws | Azamat Bukharbayev |
| `tests/TaskFlow.Domain.Tests/FunctionalExtensionsTests.cs` | Tests for curry, partial application, trampolining | Azamat Bukharbayev |
| `tests/TaskFlow.Domain.Tests/TaskPrioritizationTests.cs` | Tests for pure priority formula and ranking | Azamat Bukharbayev |
| `tests/TaskFlow.Domain.Tests/DependencyGraphTests.cs` | Tests for cycle detection and Kahn's algorithm | Azamat Bukharbayev |
| `tests/TaskFlow.Domain.Tests/ProjectProgressTests.cs` | Tests for effort-weighted calculation and recursion | Azamat Bukharbayev |
| `tests/TaskFlow.Application.Tests/ApplicationServiceTests.cs` | Tests for TaskService, ProjectService, and AnalyticsService | Azamat Bukharbayev |
| `tests/TaskFlow.Application.Tests/ImportExportTests.cs` | Tests for custom CSV escaping and JSON data serialization | Azamat Bukharbayev |
| `docs/technical/backend-architecture.md` | Clean Architecture & Functional Core specification | Azamat Bukharbayev |
| `docs/technical/functional-programming-mapping.md` | Syllabus mapping matrix for FP concepts | Azamat Bukharbayev |
| `docs/technical/functional-examples.md` | Code walkthrough for all functional paradigms | Azamat Bukharbayev |
| `docs/technical/integration-contract.md` | Public service API contract for frontend integration | Azamat Bukharbayev |
| `data/seed/initial-data.json` | Baseline demonstration dataset specification | Azamat Bukharbayev |

---

### Developer B — Ruza Isembekova (Frontend / Blazor / UX / QA / Final Report)

| Directory / File Path | Purpose & Content | Ownership |
|---|---|---|
| `TaskFlow.sln` | Master Visual Studio Solution File | Ruza Isembekova |
| `.gitignore` | Git exclusion rules for .NET, IDEs, and databases | Ruza Isembekova |
| `OWNERSHIP.md` | File ownership catalog and team assignment table | Ruza Isembekova |
| `README.md` | Comprehensive project setup and execution manual | Ruza Isembekova |
| `src/TaskFlow.Web/TaskFlow.Web.csproj` | Blazor Server web project definition | Ruza Isembekova |
| `src/TaskFlow.Web/Program.cs` | ASP.NET Core host initialization and middleware configuration | Ruza Isembekova |
| `src/TaskFlow.Web/appsettings.json` | Configuration file for SQLite connection and logging | Ruza Isembekova |
| `src/TaskFlow.Web/appsettings.Development.json` | Development environment settings | Ruza Isembekova |
| `src/TaskFlow.Web/State/TaskStateContainer.cs` | Reactive state container for UI filter synchronization | Ruza Isembekova |
| `src/TaskFlow.Web/Services/ToastService.cs` | In-app notification dispatcher | Ruza Isembekova |
| `src/TaskFlow.Web/Components/App.razor` | Blazor application root and HTML document shell | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Routes.razor` | Client-side routing table | Ruza Isembekova |
| `src/TaskFlow.Web/Components/_Imports.razor` | Razor global namespace imports | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Layout/MainLayout.razor` | Sidebar layout with top bar, team links, and toast container | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Layout/NavMenu.razor` | Responsive 7-item navigation menu | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Layout/ReconnectModal.razor` | Blazor Circuit reconnect notification | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Pages/Home.razor` | Executive Dashboard with KPI cards and productivity charts | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Pages/TasksPage.razor` | Interactive Task management with filters, sorting, and modals | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Pages/KanbanPage.razor` | 4-column visual Kanban board with instant status transitions | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Pages/ProjectsPage.razor` | Project overview, effort cards, and modal management | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Pages/CalendarPage.razor` | Monthly calendar grid with deadline indicators | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Pages/AnalyticsPage.razor` | Analytical charts, breakdown meters, and distribution metrics | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Pages/SettingsPage.razor` | CSV/JSON data import/export center and team credentials | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Pages/Error.razor` | Error handling view | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Pages/NotFound.razor` | 404 Route fallback view | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Shared/StatisticsCard.razor` | Reusable KPI metric card component | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Shared/PriorityBadge.razor` | Reusable priority badge with color accents | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Shared/StatusBadge.razor` | Reusable task status badge | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Shared/ProgressBar.razor` | Reusable percentage progress bar with color thresholds | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Shared/EmptyState.razor` | Reusable empty dataset placeholder | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Shared/TaskCard.razor` | Reusable task display card with action buttons | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Shared/ProjectCard.razor` | Reusable project display card | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Shared/TaskFormModal.razor` | Modal dialog for task creation and editing | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Shared/ProjectFormModal.razor` | Modal dialog for project creation and editing | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Shared/DependencyModal.razor` | Modal dialog for managing task dependency edges | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Shared/ConfirmationDialog.razor` | Modal confirmation dialog for destructive actions | Ruza Isembekova |
| `src/TaskFlow.Web/Components/Shared/ToastContainer.razor` | Reactive floating toast notification presenter | Ruza Isembekova |
| `src/TaskFlow.Web/wwwroot/app.css` | Comprehensive custom TaskFlow design system & CSS stylesheet | Ruza Isembekova |
| `src/TaskFlow.Web/wwwroot/js/download.js` | Client-side Base64 file download automation helper | Ruza Isembekova |
| `tests/TaskFlow.Web.Tests/WebStateAndServicesTests.cs` | Unit tests for ToastService and TaskStateContainer | Ruza Isembekova |
| `tests/TaskFlow.Integration.Tests/EndToEndIntegrationTests.cs` | Complete end-to-end integration and lifecycle tests | Ruza Isembekova |
| `docs/diagrams/system-architecture.mmd` | Mermaid diagram: Full system architecture & data flow | Ruza Isembekova |
| `docs/diagrams/database-erd.mmd` | Mermaid diagram: Entity-Relationship diagram for SQLite | Ruza Isembekova |
| `docs/diagrams/component-architecture.mmd` | Mermaid diagram: Clean Architecture & UI component tree | Ruza Isembekova |
| `docs/diagrams/workflow-diagram.mmd` | Mermaid diagram: Dependency resolution & task workflow | Ruza Isembekova |
| `docs/report/report.md` | Formal 30-section academic graduation report | Ruza Isembekova |
| `data/import-samples/sample-tasks.csv` | Sample CSV task dataset for testing import/export | Ruza Isembekova |
| `data/import-samples/sample-projects.json` | Sample JSON project dataset for testing import/export | Ruza Isembekova |
