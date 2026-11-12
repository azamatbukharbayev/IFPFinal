namespace TaskFlow.Application.Features.Projects;

using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Functional;
using TaskFlow.Domain.Services;

public class ProjectService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public ProjectService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    
    public async Task<Result<ProjectDto>> CreateProjectAsync(CreateProjectDto dto, CancellationToken ct = default)
    {
        var result = Project.Create(dto.Name, dto.Description, dto.Deadline);
        return await result.Match<Task<Result<ProjectDto>>>(
            async project =>
            {
                await _unitOfWork.Projects.AddAsync(project, ct);
                await _unitOfWork.ActivityLogs.AddAsync(
                    ActivityLog.Create("ProjectCreated", $"Project '{project.Name}' created", projectId: project.Id), ct);
                await _unitOfWork.SaveChangesAsync(ct);
                return Result<ProjectDto>.Success(await MapToDtoAsync(project, ct));
            },
            errors => Task.FromResult(Result<ProjectDto>.Failure(errors)));
    }
    
    public async Task<Result<ProjectDto>> UpdateProjectAsync(UpdateProjectDto dto, CancellationToken ct = default)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(dto.Id, ct);
        if (project is null)
            return Result<ProjectDto>.Failure($"Project with ID {dto.Id} not found.");
        
        var nameResult = project.UpdateName(dto.Name);
        if (nameResult.IsFailure)
            return Result<ProjectDto>.Failure(nameResult.Errors);
        
        project.UpdateDescription(dto.Description);
        project.UpdateDeadline(dto.Deadline);
        
        await _unitOfWork.Projects.UpdateAsync(project, ct);
        await _unitOfWork.ActivityLogs.AddAsync(
            ActivityLog.Create("ProjectUpdated", $"Project '{project.Name}' updated", projectId: project.Id), ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<ProjectDto>.Success(await MapToDtoAsync(project, ct));
    }
    
    public async Task<Result<bool>> DeleteProjectAsync(Guid id, CancellationToken ct = default)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(id, ct);
        if (project is null)
            return Result<bool>.Failure($"Project with ID {id} not found.");
        
        await _unitOfWork.Projects.DeleteAsync(id, ct);
        await _unitOfWork.ActivityLogs.AddAsync(
            ActivityLog.Create("ProjectDeleted", $"Project '{project.Name}' deleted"), ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
    
    public async Task<IReadOnlyList<ProjectDto>> GetAllProjectsAsync(CancellationToken ct = default)
    {
        var projects = await _unitOfWork.Projects.GetAllAsync(ct);
        var dtos = new List<ProjectDto>();
        foreach (var project in projects)
            dtos.Add(await MapToDtoAsync(project, ct));
        return dtos.AsReadOnly();
    }
    
    public async Task<ProjectDto?> GetProjectByIdAsync(Guid id, CancellationToken ct = default)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(id, ct);
        return project is null ? null : await MapToDtoAsync(project, ct);
    }
    
    public async Task<Result<bool>> ArchiveProjectAsync(Guid id, CancellationToken ct = default)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(id, ct);
        if (project is null)
            return Result<bool>.Failure($"Project with ID {id} not found.");
        
        project.SetStatus(ProjectStatus.Archived);
        await _unitOfWork.Projects.UpdateAsync(project, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
    
    private async Task<ProjectDto> MapToDtoAsync(Project project, CancellationToken ct)
    {
        var tasks = await _unitOfWork.Tasks.GetByProjectIdAsync(project.Id, ct);
        var progress = ProjectProgressService.CalculateProgress(tasks.ToList());
        
        return new ProjectDto(
            project.Id, project.Name, project.Description,
            project.Status, project.CreatedAt, project.Deadline,
            progress.TotalTasks, progress.CompletedTasks,
            progress.CompletionPercentage,
            progress.TotalEffort, progress.CompletedEffort);
    }
}
