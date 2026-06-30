using System.Text.Json;

namespace UiCodeGenerator.Application.DTOs;

public sealed record HistorySessionResponse(
    string Id,
    string SchemaName,
    string Prompt,
    JsonElement Schema,
    DateTimeOffset CreatedAt);
