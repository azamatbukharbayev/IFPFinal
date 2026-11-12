using TaskFlow.Web.Components;
using TaskFlow.Web.State;
using TaskFlow.Web.Services;
using TaskFlow.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// TaskFlow infrastructure + application services
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=taskflow.db";
builder.Services.AddTaskFlowInfrastructure(connectionString);

// UI services
builder.Services.AddSingleton<TaskStateContainer>();
builder.Services.AddSingleton<ToastService>();

var app = builder.Build();

// Seed database
await DependencyInjection.SeedDatabaseAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public partial class Program { } // For integration tests
