namespace TaskFlow.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

public class ProjectRepository : IProjectRepository
{
    private readonly TaskFlowDbContext _context;
    
    public ProjectRepository(TaskFlowDbContext context) => _context = context;
    
    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    
    public async Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Projects
            .Include(p => p.Tasks)
            .AsNoTracking()
            .ToListAsync(ct);
    
    public async Task AddAsync(Project project, CancellationToken ct = default) =>
        await _context.Projects.AddAsync(project, ct);
    
    public Task UpdateAsync(Project project, CancellationToken ct = default)
    {
        _context.Projects.Update(project);
        return Task.CompletedTask;
    }
    
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var project = await _context.Projects.FindAsync(new object[] { id }, ct);
        if (project is not null)
            _context.Projects.Remove(project);
    }
}
