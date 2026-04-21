using System.Collections.Concurrent;
using NLW.Application.Common.Interfaces;
using NLW.Domain.Entities;

namespace NLW.Infrastructure.Persistence;

public sealed class InMemoryWireframeRepository : IWireframeRepository
{
    private readonly ConcurrentDictionary<string, WireframeSchema> _store = new();

    public Task SaveAsync(WireframeSchema schema, CancellationToken ct = default)
    {
        _store[schema.Id] = schema;
        return Task.CompletedTask;
    }

    public Task<WireframeSchema?> GetByIdAsync(string id, CancellationToken ct = default)
        => Task.FromResult(_store.TryGetValue(id, out var schema) ? schema : null);

    public Task<IReadOnlyList<WireframeSchema>> GetRecentAsync(int count = 20, CancellationToken ct = default)
    {
        var result = _store.Values
            .OrderByDescending(s => s.CreatedAt)
            .Take(count)
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyList<WireframeSchema>>(result);
    }
}
