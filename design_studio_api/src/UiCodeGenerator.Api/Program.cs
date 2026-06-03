using Microsoft.OpenApi.Models;

using UiCodeGenerator.Api.Middleware;
using UiCodeGenerator.Application.Abstractions;
using UiCodeGenerator.Application.Services;
using UiCodeGenerator.Infrastructure;

const string CorsPolicyName = "Frontend";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UI Code Generator API",
        Version = "v1",
        Description = "Generates UI preview data and source code from natural-language prompts using DeepSeek."
    }));

builder.Services.AddScoped<IUiGenerationService, UiGenerationService>();
builder.Services.AddInfrastructure(builder.Configuration);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .GetChildren()
    .Select(origin => origin.Value)
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Select(origin => origin!)
    .ToArray();

builder.Services.AddCors(options =>
    options.AddPolicy(CorsPolicyName, policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();

            return;
        }

        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    }));

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "UI Code Generator API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseCors(CorsPolicyName);

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "UiCodeGenerator.Api",
    utc = DateTimeOffset.UtcNow
}))
.WithName("Health");

app.Run();