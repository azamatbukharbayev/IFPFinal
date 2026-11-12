namespace TaskFlow.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;

public class TaskFlowDbContext : DbContext
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskDependency> TaskDependencies => Set<TaskDependency>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    
    public TaskFlowDbContext(DbContextOptions<TaskFlowDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskFlowDbContext).Assembly);
    }
}
