namespace NLW.Application.Common.Interfaces;

public interface IComponentIndexService
{
    Task<IReadOnlyList<ComponentMetadata>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ComponentMetadata>> SearchAsync(string query, int topN = 10, CancellationToken ct = default);
}

public sealed record ComponentMetadata(
    string Selector,
    string[] Inputs,
    string[] Outputs,
    string Description,
    string Category,
    string MappedBlockType
);
