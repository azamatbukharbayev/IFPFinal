namespace TaskFlow.Application.Tests;

using System.Text;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Infrastructure.ImportExport;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Repositories;
using Xunit;

public class ImportExportTests : IDisposable
{
    private readonly TaskFlowDbContext _context;
    private readonly UnitOfWork _uow;
    private readonly ImportExportService _service;

    public ImportExportTests()
    {
        var options = new DbContextOptionsBuilder<TaskFlowDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TaskFlowDbContext(options);
        _uow = new UnitOfWork(_context);
        _service = new ImportExportService(_uow);
    }

    public void Dispose()
    {
        _uow.Dispose();
        _context.Dispose();
    }

    [Fact]
    public void ParseCsvLines_HandlesQuotedFieldsWithCommas()
    {
        var csv = "Title,Description,Priority\n\"Hello, World!\",\"A task, with commas\",High\nSimple,NoQuotes,Low";
        var lines = ImportExportService.ParseCsvLines(csv);

        Assert.Equal(3, lines.Count);
        Assert.Equal("Title", lines[0][0]);
        Assert.Equal("Hello, World!", lines[1][0]);
        Assert.Equal("A task, with commas", lines[1][1]);
        Assert.Equal("High", lines[1][2]);
        Assert.Equal("Simple", lines[2][0]);
    }

    [Fact]
    public async Task ImportTasksFromCsv_ImportsValidRows()
    {
        var csv = "Title,Description,Priority,Deadline,ProjectName,EstimatedEffortHours,Labels\n" +
                  "Test Task 1,Some description,Critical,2026-12-01,,4.5,tag1;tag2\n" +
                  "Test Task 2,Another one,Low,2026-12-10,,2.0,backend\n";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var result = await _service.ImportTasksFromCsvAsync(stream);

        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);

        var tasks = await _uow.Tasks.GetAllAsync();
        Assert.Equal(2, tasks.Count);
    }

    [Fact]
    public async Task ExportTasksToCsv_ProducesParseableCsv()
    {
        var csv = "Title,Description,Priority,Deadline,ProjectName,EstimatedEffortHours,Labels\n" +
                  "Task Alpha,Description Alpha,High,2026-11-20,,5.0,alpha\n";

        using var importStream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        await _service.ImportTasksFromCsvAsync(importStream);

        var exportedBytes = await _service.ExportTasksToCsvAsync();
        var exportedText = Encoding.UTF8.GetString(exportedBytes);

        Assert.Contains("Task Alpha", exportedText);
        Assert.Contains("Description Alpha", exportedText);
    }

    [Fact]
    public async Task ImportProjectsFromJson_ImportsProjectsCorrectly()
    {
        var json = @"[
            { ""Name"": ""Project Alpha"", ""Description"": ""Alpha Description"", ""Deadline"": ""2026-12-31T00:00:00Z"" },
            { ""Name"": ""Project Beta"", ""Description"": ""Beta Description"", ""Deadline"": null }
        ]";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var result = await _service.ImportProjectsFromJsonAsync(stream);

        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);

        var projects = await _uow.Projects.GetAllAsync();
        Assert.Equal(2, projects.Count);
    }
}
