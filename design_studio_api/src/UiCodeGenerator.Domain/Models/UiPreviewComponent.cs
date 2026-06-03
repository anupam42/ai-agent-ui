namespace UiCodeGenerator.Domain.Models;

public sealed class UiPreviewComponent
{
    public string Id { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string? Label { get; set; }

    public string? Text { get; set; }

    public Dictionary<string, object?> Props { get; set; } = [];

    public List<UiPreviewComponent> Children { get; set; } = [];
}