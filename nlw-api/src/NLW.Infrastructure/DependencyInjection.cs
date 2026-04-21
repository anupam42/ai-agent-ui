using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLW.Application.Common.Interfaces;
using NLW.Infrastructure.AI;
using NLW.Infrastructure.ComponentIndex;
using NLW.Infrastructure.Configuration;
using NLW.Infrastructure.Persistence;

namespace NLW.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var aiSettings = configuration.GetSection(AISettings.SectionName).Get<AISettings>()
            ?? throw new InvalidOperationException("AI configuration section is missing.");

        services.Configure<AISettings>(configuration.GetSection(AISettings.SectionName));

        services.AddHttpClient<IAIModelService, ClaudeAIService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(aiSettings.TimeoutSeconds);
            client.BaseAddress = new Uri(aiSettings.BaseUrl);
        });

        services.AddMemoryCache();
        services.AddSingleton<IComponentIndexService, ComponentIndexService>();
        services.AddScoped<IPromptBuilderService, PromptBuilderService>();
        services.AddSingleton<IWireframeRepository, InMemoryWireframeRepository>();

        return services;
    }
}
