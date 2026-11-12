namespace TaskFlow.Application.DTOs;

using TaskFlow.Domain.Enums;

public record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    ProjectStatus Status,
    DateTime CreatedAt,
    DateTime? Deadline,
    int TaskCount,
    int CompletedTaskCount,
    double CompletionPercentage,
    decimal TotalEffort,
    decimal CompletedEffort);
