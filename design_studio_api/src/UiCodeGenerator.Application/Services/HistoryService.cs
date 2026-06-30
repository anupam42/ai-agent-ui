using System.Text.Json;

using UiCodeGenerator.Application.Abstractions;
using UiCodeGenerator.Application.DTOs;
using UiCodeGenerator.Domain.Models;

namespace UiCodeGenerator.Application.Services;

public sealed class HistoryService(IHistoryRepository repository) : IHistoryService
{
    public async Task<IReadOnlyList<HistorySessionResponse>> GetAllAsync(string? search, CancellationToken ct)
    {
        var sessions = await repository.GetAllAsync(search, ct);
        return sessions.Select(ToResponse).ToList();
    }

    public async Task<HistorySessionResponse> SaveAsync(SaveHistorySessionRequest request, CancellationToken ct)
    {
        var session = new HistorySession
        {
            Id = request.Id,
            SchemaName = request.SchemaName,
            Prompt = request.Prompt,
            SchemaJson = request.Schema.GetRawText(),
            CreatedAt = request.CreatedAt == default ? DateTimeOffset.UtcNow : request.CreatedAt,
        };

        await repository.AddOrUpdateAsync(session, ct);
        return ToResponse(session);
    }

    public Task<bool> DeleteAsync(string id, CancellationToken ct)
        => repository.DeleteAsync(id, ct);

    public Task ClearAllAsync(CancellationToken ct)
        => repository.ClearAllAsync(ct);

    private static HistorySessionResponse ToResponse(HistorySession s) => new(
        s.Id,
        s.SchemaName,
        s.Prompt,
        JsonSerializer.Deserialize<JsonElement>(s.SchemaJson),
        s.CreatedAt);
}
