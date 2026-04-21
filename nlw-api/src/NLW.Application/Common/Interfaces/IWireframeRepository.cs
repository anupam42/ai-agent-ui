using NLW.Domain.Entities;

namespace NLW.Application.Common.Interfaces;

public interface IWireframeRepository
{
    Task SaveAsync(WireframeSchema schema, CancellationToken ct = default);
    Task<WireframeSchema?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<WireframeSchema>> GetRecentAsync(int count = 20, CancellationToken ct = default);
}
