namespace TaskFlow.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(2000);
        builder.Property(t => t.Status).HasConversion<string>();
        builder.Property(t => t.Priority).HasConversion<string>();
        builder.Property(t => t.EstimatedEffortHours).HasPrecision(10, 2);
        builder.Property(t => t.LabelsJson).HasMaxLength(1000).HasDefaultValue("[]");
        
        builder.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasOne(t => t.ParentTask)
            .WithMany(t => t.SubTasks)
            .HasForeignKey(t => t.ParentTaskId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(t => t.DependsOn)
            .WithOne(d => d.Task)
            .HasForeignKey(d => d.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(t => t.DependedOnBy)
            .WithOne(d => d.DependsOnTask)
            .HasForeignKey(d => d.DependsOnTaskId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Priority);
        builder.HasIndex(t => t.ProjectId);
        builder.HasIndex(t => t.Deadline);
    }
}
