using System.Net.Http.Headers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using UiCodeGenerator.Application.Abstractions;
using UiCodeGenerator.Infrastructure.DeepSeek;

namespace UiCodeGenerator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<DeepSeekOptions>()
            .Bind(configuration.GetSection(DeepSeekOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                options => Uri.TryCreate(
                    options.BaseUrl,
                    UriKind.Absolute,
                    out _),
                "DeepSeek:BaseUrl must be an absolute URL.")
            .Validate(
                options =>
                    string.Equals(
                        options.ReasoningEffort,
                        "high",
                        StringComparison.OrdinalIgnoreCase)
                    || string.Equals(
                        options.ReasoningEffort,
                        "max",
                        StringComparison.OrdinalIgnoreCase),
                "DeepSeek:ReasoningEffort must be high or max.");

        services.AddHttpClient<IAiUiCodeGenerator, DeepSeekUiCodeGenerator>(
            (serviceProvider, httpClient) =>
            {
                var options =
                    serviceProvider
                        .GetRequiredService<IOptions<DeepSeekOptions>>()
                        .Value;

                var baseUrl =
                    options.BaseUrl.EndsWith('/')
                        ? options.BaseUrl
                        : $"{options.BaseUrl}/";

                httpClient.BaseAddress = new Uri(baseUrl);
                httpClient.Timeout =
                    TimeSpan.FromSeconds(options.TimeoutSeconds);

                httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(
                        "application/json"));
            });

        return services;
    }
}