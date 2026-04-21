using System.Net;
using System.Text.Json;
using FluentValidation;

namespace NLW.Api.Middleware;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed: {Errors}", ex.Errors);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = "ValidationFailure",
                title = "One or more validation errors occurred.",
                status = 400,
                errors
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = context.RequestServices
                .GetRequiredService<IHostEnvironment>().IsDevelopment()
                ? new { title = ex.Message, detail = ex.StackTrace, status = 500 }
                : new { title = "An internal error occurred.", detail = (string?)null, status = 500 };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
