namespace TaskFlow.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

public class TaskDependencyConfiguration : IEntityTypeConfiguration<TaskDependency>
{
    public void Configure(EntityTypeBuilder<TaskDependency> builder)
    {
        builder.HasKey(d => d.Id);
        builder.HasIndex(d => new { d.TaskId, d.DependsOnTaskId }).IsUnique();
    }
}
