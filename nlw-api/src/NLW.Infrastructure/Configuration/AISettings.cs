namespace NLW.Infrastructure.Configuration;

public sealed class AISettings
{
    public const string SectionName = "AI";

    public string Provider { get; set; } = "Claude";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-sonnet-4-6";
    public string BaseUrl { get; set; } = "https://api.anthropic.com";
    public int MaxTokens { get; set; } = 4096;
    public int TimeoutSeconds { get; set; } = 60;
}
