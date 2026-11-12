namespace TaskFlow.Application.Interfaces;

using TaskFlow.Application.DTOs;

public interface IImportExportService
{
    Task<ImportResultDto> ImportTasksFromCsvAsync(Stream csvStream, CancellationToken ct = default);
    Task<ImportResultDto> ImportProjectsFromJsonAsync(Stream jsonStream, CancellationToken ct = default);
    Task<byte[]> ExportTasksToCsvAsync(CancellationToken ct = default);
    Task<byte[]> ExportTasksToJsonAsync(CancellationToken ct = default);
}
