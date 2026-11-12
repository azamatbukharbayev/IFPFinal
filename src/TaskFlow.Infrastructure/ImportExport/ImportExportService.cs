namespace TaskFlow.Infrastructure.ImportExport;

using System.Globalization;
using System.Text;
using System.Text.Json;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

public class ImportExportService : IImportExportService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public ImportExportService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    
    public async Task<ImportResultDto> ImportTasksFromCsvAsync(Stream csvStream, CancellationToken ct = default)
    {
        var errors = new List<string>();
        var successCount = 0;
        
        using var reader = new StreamReader(csvStream, Encoding.UTF8);
        var content = await reader.ReadToEndAsync(ct);
        var lines = ParseCsvLines(content);
        
        if (lines.Count <= 1)
            return new ImportResultDto(0, 0, new List<string> { "CSV file is empty or only contains headers." });
        
        // Skip header
        var dataRows = lines.Skip(1);
        
        foreach (var row in dataRows)
        {
            if (row.Count == 0 || (row.Count == 1 && string.IsNullOrWhiteSpace(row[0])))
                continue;
            
            try
            {
                var title = row.ElementAtOrDefault(0) ?? string.Empty;
                var description = row.ElementAtOrDefault(1);
                var priorityStr = row.ElementAtOrDefault(2) ?? "Medium";
                var deadlineStr = row.ElementAtOrDefault(3);
                var effortStr = row.ElementAtOrDefault(5) ?? "0";
                var labelsStr = row.ElementAtOrDefault(6);
                
                var priority = Enum.TryParse<TaskPriority>(priorityStr, true, out var p) ? p : TaskPriority.Medium;
                DateTime? deadline = null;
                if (!string.IsNullOrWhiteSpace(deadlineStr) && DateTime.TryParse(deadlineStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedDate))
                {
                    deadline = parsedDate.ToUniversalTime();
                }
                
                decimal.TryParse(effortStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var effort);
                var labels = string.IsNullOrWhiteSpace(labelsStr)
                    ? new List<string>()
                    : labelsStr.Split(';').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToList();
                
                var result = TaskItem.Create(title, description, priority, deadline, null, effort, labels);
                result.Match(
                    task =>
                    {
                        _unitOfWork.Tasks.AddAsync(task, ct).GetAwaiter().GetResult();
                        successCount++;
                    },
                    errs => errors.AddRange(errs.Select(e => $"Row '{title}': {e}")));
            }
            catch (Exception ex)
            {
                errors.Add($"Row error: {ex.Message}");
            }
        }
        
        if (successCount > 0)
            await _unitOfWork.SaveChangesAsync(ct);
        
        return new ImportResultDto(successCount, errors.Count, errors);
    }
    
    public async Task<ImportResultDto> ImportProjectsFromJsonAsync(Stream jsonStream, CancellationToken ct = default)
    {
        var errors = new List<string>();
        var successCount = 0;
        
        var projectDtos = await JsonSerializer.DeserializeAsync<List<CreateProjectDto>>(jsonStream, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);
        
        if (projectDtos is null)
            return new ImportResultDto(0, 1, new List<string> { "Invalid JSON format." });
        
        foreach (var dto in projectDtos)
        {
            var result = Project.Create(dto.Name, dto.Description, dto.Deadline);
            result.Match(
                project =>
                {
                    _unitOfWork.Projects.AddAsync(project, ct).GetAwaiter().GetResult();
                    successCount++;
                },
                errs => errors.AddRange(errs.Select(e => $"Project '{dto.Name}': {e}")));
        }
        
        if (successCount > 0)
            await _unitOfWork.SaveChangesAsync(ct);
        
        return new ImportResultDto(successCount, errors.Count, errors);
    }
    
    public async Task<byte[]> ExportTasksToCsvAsync(CancellationToken ct = default)
    {
        var tasks = await _unitOfWork.Tasks.GetAllAsync(ct);
        
        var sb = new StringBuilder();
        sb.AppendLine("Title,Description,Priority,Deadline,ProjectName,EstimatedEffortHours,Labels");
        
        foreach (var t in tasks)
        {
            var title = EscapeCsvField(t.Title);
            var desc = EscapeCsvField(t.Description ?? string.Empty);
            var priority = t.Priority.ToString();
            var deadline = t.Deadline?.ToString("yyyy-MM-dd") ?? string.Empty;
            var projectName = EscapeCsvField(t.Project?.Name ?? string.Empty);
            var effort = t.EstimatedEffortHours.ToString(CultureInfo.InvariantCulture);
            var labels = EscapeCsvField(string.Join(";", t.GetLabels()));
            
            sb.AppendLine($"{title},{desc},{priority},{deadline},{projectName},{effort},{labels}");
        }
        
        return Encoding.UTF8.GetBytes(sb.ToString());
    }
    
    public async Task<byte[]> ExportTasksToJsonAsync(CancellationToken ct = default)
    {
        var tasks = await _unitOfWork.Tasks.GetAllAsync(ct);
        var dtos = tasks.Select(t => new
        {
            t.Id,
            t.Title,
            t.Description,
            Status = t.Status.ToString(),
            Priority = t.Priority.ToString(),
            t.CreatedAt,
            t.Deadline,
            t.CompletedAt,
            t.ProjectId,
            ProjectName = t.Project?.Name,
            t.EstimatedEffortHours,
            Labels = t.GetLabels()
        }).ToList();
        
        return JsonSerializer.SerializeToUtf8Bytes(dtos, new JsonSerializerOptions { WriteIndented = true });
    }
    
    // Pure functional CSV line parser supporting quoted values with commas
    public static List<List<string>> ParseCsvLines(string csvText)
    {
        var result = new List<List<string>>();
        if (string.IsNullOrWhiteSpace(csvText))
            return result;
        
        var currentRow = new List<string>();
        var currentField = new StringBuilder();
        var inQuotes = false;
        
        for (var i = 0; i < csvText.Length; i++)
        {
            var c = csvText[i];
            
            if (c == '"')
            {
                if (inQuotes && i + 1 < csvText.Length && csvText[i + 1] == '"')
                {
                    currentField.Append('"');
                    i++; // skip escaped quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                currentRow.Add(currentField.ToString());
                currentField.Clear();
            }
            else if ((c == '\r' || c == '\n') && !inQuotes)
            {
                if (c == '\r' && i + 1 < csvText.Length && csvText[i + 1] == '\n')
                    i++;
                
                currentRow.Add(currentField.ToString());
                currentField.Clear();
                result.Add(currentRow);
                currentRow = new List<string>();
            }
            else
            {
                currentField.Append(c);
            }
        }
        
        if (currentField.Length > 0 || currentRow.Count > 0)
        {
            currentRow.Add(currentField.ToString());
            result.Add(currentRow);
        }
        
        return result;
    }
    
    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;
        
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
