namespace TaskFlow.Infrastructure.Repositories;

using TaskFlow.Application.Interfaces;
using TaskFlow.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly TaskFlowDbContext _context;
    private ITaskRepository? _tasks;
    private IProjectRepository? _projects;
    private IActivityLogRepository? _activityLogs;
    
    public UnitOfWork(TaskFlowDbContext context) => _context = context;
    
    public ITaskRepository Tasks => _tasks ??= new TaskRepository(_context);
    public IProjectRepository Projects => _projects ??= new ProjectRepository(_context);
    public IActivityLogRepository ActivityLogs => _activityLogs ??= new ActivityLogRepository(_context);
    
    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
    
    public void Dispose() => _context.Dispose();
    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
}
