namespace TaskFlow.Application.Interfaces;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    ITaskRepository Tasks { get; }
    IProjectRepository Projects { get; }
    IActivityLogRepository ActivityLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
