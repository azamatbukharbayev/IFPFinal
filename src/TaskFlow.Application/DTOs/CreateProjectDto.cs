namespace TaskFlow.Application.DTOs;

public record CreateProjectDto(
    string Name,
    string? Description,
    DateTime? Deadline);
