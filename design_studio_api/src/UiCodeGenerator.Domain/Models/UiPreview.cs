namespace UiCodeGenerator.Domain.Models;

public sealed class UiPreview
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Layout { get; set; } = "single-screen";

    public UiTheme Theme { get; set; } = new();

    public List<UiPreviewComponent> Components { get; set; } = [];
}
