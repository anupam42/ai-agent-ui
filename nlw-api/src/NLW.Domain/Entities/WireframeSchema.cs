using NLW.Domain.ValueObjects;

namespace NLW.Domain.Entities;

public sealed class WireframeSchema
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public string Prompt { get; private set; }
    public WireframeNode Root { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<ComponentMapping> Mappings { get; private set; }

    private WireframeSchema() { }

    public static WireframeSchema Create(
        string name,
        string prompt,
        WireframeNode root,
        IEnumerable<ComponentMapping> mappings)
    {
        return new WireframeSchema
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Prompt = prompt,
            Root = root,
            CreatedAt = DateTime.UtcNow,
            Mappings = mappings.ToList().AsReadOnly()
        };
    }
}
