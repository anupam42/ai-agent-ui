using System.ComponentModel.DataAnnotations;

namespace UiCodeGenerator.Infrastructure.DeepSeek;

public sealed class DeepSeekOptions
{
    public const string SectionName = "DeepSeek";

    [Required]
    [Url]
    public string BaseUrl { get; init; } = "https://api.deepseek.com";

    [Required]
    public string Model { get; init; } = "deepseek-v4-flash";

    public string? ApiKey { get; init; }

    [Range(1, 384000)]
    public int MaxTokens { get; init; } = 6000;

    [Range(0, 2)]
    public double Temperature { get; init; } = 0.2;

    public bool EnableThinking { get; init; }

    [Required]
    public string ReasoningEffort { get; init; } = "high";

    [Range(5, 300)]
    public int TimeoutSeconds { get; init; } = 90;
}
