namespace TaskFlow.Application.Interfaces;

using TaskFlow.Domain.Entities;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Project project, CancellationToken ct = default);
    Task UpdateAsync(Project project, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
