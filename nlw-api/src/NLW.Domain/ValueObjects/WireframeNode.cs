using NLW.Domain.Enums;

namespace NLW.Domain.ValueObjects;

public sealed record WireframeNode
{
    public string? Id { get; init; }
    public WireframeBlockType Type { get; init; }
    public string? Label { get; init; }
    public IReadOnlyList<WireframeNode>? Children { get; init; }
    public int? Span { get; init; }
    public string? Width { get; init; }
    public string? Height { get; init; }
    public bool? Sticky { get; init; }
    public IReadOnlyList<string>? Items { get; init; }
    public IReadOnlyList<string>? Columns { get; init; }
    public int? Rows { get; init; }
    public string? Title { get; init; }
    public string? Value { get; init; }
    public string? MappedComponent { get; init; }
    public string? Placeholder { get; init; }
}
