using MediatR;
using Microsoft.Extensions.Logging;

namespace NLW.Application;

public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName}", name);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var response = await next();
        sw.Stop();
        _logger.LogInformation("Handled {RequestName} in {Elapsed}ms", name, sw.ElapsedMilliseconds);
        return response;
    }
}
