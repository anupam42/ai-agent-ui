using NLW.Domain.Enums;

namespace NLW.Domain.Entities;

public sealed class ComponentMapping
{
    public string BlockLabel { get; init; } = string.Empty;
    public WireframeBlockType BlockType { get; init; }
    public string ComponentName { get; init; } = string.Empty;
    public MatchConfidence MatchConfidence { get; init; }
}
