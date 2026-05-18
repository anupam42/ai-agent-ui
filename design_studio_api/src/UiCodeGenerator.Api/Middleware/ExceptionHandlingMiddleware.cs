using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Mvc;
using UiCodeGenerator.Application.Exceptions;

namespace UiCodeGenerator.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation("Request was canceled by the client.");
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }

        var problem = CreateProblemDetails(context, exception);

        if (problem.Status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled API exception.");
        }
        else
        {
            logger.LogWarning(exception, "API exception mapped to {StatusCode}.", problem.Status);
        }

        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem, context.RequestAborted);
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var (status, title, detail) = exception switch
        {
            AiConfigurationException => (
                StatusCodes.Status500InternalServerError,
                "AI provider is not configured",
                exception.Message),
            AiProviderException providerException => MapProviderException(providerException),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Unexpected server error",
                environment.IsDevelopment() ? exception.Message : "An unexpected error occurred.")
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;

        if (exception is AiProviderException provider && provider.ProviderStatusCode.HasValue)
        {
            problem.Extensions["providerStatusCode"] = provider.ProviderStatusCode.Value;
        }

        return problem;
    }

    private (int Status, string Title, string Detail) MapProviderException(AiProviderException exception)
    {
        return exception.ProviderStatusCode switch
        {
            StatusCodes.Status429TooManyRequests => (
                StatusCodes.Status429TooManyRequests,
                "AI provider rate limit reached",
                exception.Message),
            StatusCodes.Status500InternalServerError or StatusCodes.Status503ServiceUnavailable => (
                StatusCodes.Status503ServiceUnavailable,
                "AI provider is unavailable",
                environment.IsDevelopment() ? exception.Message : "The AI provider is temporarily unavailable."),
            _ => (
                StatusCodes.Status502BadGateway,
                "AI provider request failed",
                environment.IsDevelopment() ? exception.Message : "The AI provider request failed.")
        };
    }
}
