namespace TaskFlow.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly TaskFlowDbContext _context;
    
    public ActivityLogRepository(TaskFlowDbContext context) => _context = context;
    
    public async Task<IReadOnlyList<ActivityLog>> GetRecentAsync(int count = 20, CancellationToken ct = default) =>
        await _context.ActivityLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(ct);
    
    public async Task<IReadOnlyList<ActivityLog>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default) =>
        await _context.ActivityLogs
            .Where(a => a.TaskId == taskId)
            .OrderByDescending(a => a.Timestamp)
            .AsNoTracking()
            .ToListAsync(ct);
    
    public async Task AddAsync(ActivityLog log, CancellationToken ct = default) =>
        await _context.ActivityLogs.AddAsync(log, ct);
}
