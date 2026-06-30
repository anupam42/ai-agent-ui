namespace UiCodeGenerator.Domain.Models;

public sealed class HistorySession
{
    public string Id { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string SchemaJson { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
