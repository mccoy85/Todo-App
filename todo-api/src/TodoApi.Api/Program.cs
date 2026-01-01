using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Api.Middleware;
using TodoApi.Api.Validators;
using TodoApi.Core.Interfaces;
using TodoApi.Core.Services;
using TodoApi.Infrastructure.Data;
using TodoApi.Infrastructure.Repositories;

// App startup: DI, middleware, and endpoint configuration.
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
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTodoRequestValidator>();
// Customize validation error responses to include all errors in a structured format
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        var response = new ErrorResponse
        {
            StatusCode = StatusCodes.Status400BadRequest,
            Message = "Validation failed",
            Type = "ValidationError",
            Errors = errors
        };

        return new BadRequestObjectResult(response);
    };
});
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
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? ["http://localhost:3000"];

    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
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

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
