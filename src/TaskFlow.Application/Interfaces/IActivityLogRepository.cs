namespace TaskFlow.Application.Interfaces;

using TaskFlow.Domain.Entities;

public interface IActivityLogRepository
{
    Task<IReadOnlyList<ActivityLog>> GetRecentAsync(int count = 20, CancellationToken ct = default);
    Task<IReadOnlyList<ActivityLog>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default);
    Task AddAsync(ActivityLog log, CancellationToken ct = default);
}
