using Microsoft.EntityFrameworkCore;
using TodoApi.Api.Middleware;
using TodoApi.Core.Interfaces;
using TodoApi.Core.Services;
using TodoApi.Infrastructure.Data;
using TodoApi.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use host/port from configuration or environment variables
// Skip URL configuration in testing environment (WebApplicationFactory handles this)
if (!builder.Environment.IsEnvironment("Testing"))
{
    var host = builder.Configuration["Api:Host"] ?? "localhost";
    var port = builder.Configuration.GetValue<int>("Api:Port", 5121);
    builder.WebHost.UseUrls($"http://{host}:{port}");
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Todo API",
        Version = "v1",
        Description = "A RESTful API for managing todo items with priority levels, due dates, filtering, and sorting."
    });
});

// Configure SQLite database
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=todo.db"));

// Register repositories
builder.Services.AddScoped<ITodoRepository, TodoRepository>();

// Register services
builder.Services.AddScoped<ITodoService, TodoService>();

// Configure CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Apply migrations automatically in Development (skip in testing - handled by test factory)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    context.Database.Migrate();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "api/docs/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/api/docs/v1/swagger.json", "Todo API v1");
        options.RoutePrefix = "api/docs";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
