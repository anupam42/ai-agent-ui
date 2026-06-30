using UiCodeGenerator.Domain.Models;

namespace UiCodeGenerator.Application.Abstractions;

public interface IHistoryRepository
{
    Task<IReadOnlyList<HistorySession>> GetAllAsync(string? search, CancellationToken ct);
    Task AddOrUpdateAsync(HistorySession session, CancellationToken ct);
    Task<bool> DeleteAsync(string id, CancellationToken ct);
    Task ClearAllAsync(CancellationToken ct);
}
