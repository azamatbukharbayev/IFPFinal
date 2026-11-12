namespace TaskFlow.Application.DTOs;

using TaskFlow.Domain.Enums;

public record UpdateTaskDto(
    Guid Id,
    string Title,
    string? Description,
    TaskPriority Priority,
    DateTime? Deadline,
    Guid? ProjectId,
    decimal EstimatedEffortHours,
    List<string>? Labels,
    Guid? ParentTaskId);
