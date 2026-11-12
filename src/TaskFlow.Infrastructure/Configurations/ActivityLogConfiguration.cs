namespace TaskFlow.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Action).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Details).HasMaxLength(1000);
        builder.HasIndex(a => a.Timestamp);
        
        builder.HasOne(a => a.Task)
            .WithMany(t => t.ActivityLogs)
            .HasForeignKey(a => a.TaskId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
