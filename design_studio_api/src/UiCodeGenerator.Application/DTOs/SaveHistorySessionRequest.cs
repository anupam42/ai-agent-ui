using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace UiCodeGenerator.Application.DTOs;

public sealed class SaveHistorySessionRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Id { get; init; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string SchemaName { get; init; } = string.Empty;

    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public string Prompt { get; init; } = string.Empty;

    public JsonElement Schema { get; init; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
