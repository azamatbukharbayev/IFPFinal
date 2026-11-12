namespace TaskFlow.Application.DTOs;

public record TaskCsvRow(
    string Title,
    string? Description,
    string Priority,
    string? Deadline,
    string? ProjectName,
    decimal EstimatedEffortHours,
    string? Labels);

public record ImportResultDto(int SuccessCount, int FailureCount, List<string> Errors);
public record ExportResultDto(string FilePath, int RecordCount, string Format);
