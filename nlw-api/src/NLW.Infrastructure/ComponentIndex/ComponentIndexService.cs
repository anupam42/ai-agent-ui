using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NLW.Application.Common.Interfaces;

namespace NLW.Infrastructure.ComponentIndex;

public sealed class ComponentIndexService : IComponentIndexService
{
    private const string CacheKey = "component_index";
    private readonly IMemoryCache _cache;
    private readonly ILogger<ComponentIndexService> _logger;

    public ComponentIndexService(IMemoryCache cache, ILogger<ComponentIndexService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ComponentMetadata>> GetAllAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue<IReadOnlyList<ComponentMetadata>>(CacheKey, out var cached) && cached is not null)
            return cached;

        var components = await LoadFromEmbeddedResourceAsync(ct);
        _cache.Set(CacheKey, components, TimeSpan.FromHours(1));
        return components;
    }

    public async Task<IReadOnlyList<ComponentMetadata>> SearchAsync(
        string query, int topN = 10, CancellationToken ct = default)
    {
        var all = await GetAllAsync(ct);
        var q = query.ToLowerInvariant();

        return all
            .Select(c => new
            {
                Component = c,
                Score = Score(c, q)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(topN)
            .Select(x => x.Component)
            .ToList()
            .AsReadOnly();
    }

    private static int Score(ComponentMetadata c, string query)
    {
        var score = 0;
        if (c.Description.Contains(query, StringComparison.OrdinalIgnoreCase)) score += 3;
        if (c.Category.Contains(query, StringComparison.OrdinalIgnoreCase)) score += 2;
        if (c.Selector.Contains(query, StringComparison.OrdinalIgnoreCase)) score += 2;
        if (c.MappedBlockType.Contains(query, StringComparison.OrdinalIgnoreCase)) score += 1;
        return score;
    }

    private async Task<IReadOnlyList<ComponentMetadata>> LoadFromEmbeddedResourceAsync(CancellationToken ct)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("component-index.json"))
                ?? throw new FileNotFoundException("component-index.json embedded resource not found.");

            await using var stream = assembly.GetManifestResourceStream(resourceName)!;
            var items = await JsonSerializer.DeserializeAsync<List<ComponentIndexEntry>>(stream, cancellationToken: ct) ?? [];

            return items.Select(i => new ComponentMetadata(
                i.Selector,
                i.Inputs ?? [],
                i.Outputs ?? [],
                i.Description,
                i.Category,
                i.MappedBlockType
            )).ToList().AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load component index, returning empty list");
            return [];
        }
    }

    private sealed record ComponentIndexEntry(
        string Selector,
        string[]? Inputs,
        string[]? Outputs,
        string Description,
        string Category,
        string MappedBlockType
    );
}
