namespace TaskFlow.Application.DTOs;

using TaskFlow.Domain.Enums;

public record TaskItemDto(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime CreatedAt,
    DateTime? Deadline,
    DateTime? CompletedAt,
    Guid? ProjectId,
    string? ProjectName,
    decimal EstimatedEffortHours,
    List<string> Labels,
    Guid? ParentTaskId,
    List<Guid> DependencyIds,
    double? PriorityScore,
    string? DeadlineClassification);
