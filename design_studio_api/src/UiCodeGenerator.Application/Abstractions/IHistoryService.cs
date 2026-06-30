using UiCodeGenerator.Application.DTOs;

namespace UiCodeGenerator.Application.Abstractions;

public interface IHistoryService
{
    Task<IReadOnlyList<HistorySessionResponse>> GetAllAsync(string? search, CancellationToken ct);
    Task<HistorySessionResponse> SaveAsync(SaveHistorySessionRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(string id, CancellationToken ct);
    Task ClearAllAsync(CancellationToken ct);
}
