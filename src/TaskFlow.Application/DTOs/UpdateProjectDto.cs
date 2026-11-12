namespace TaskFlow.Application.DTOs;

public record UpdateProjectDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime? Deadline);
