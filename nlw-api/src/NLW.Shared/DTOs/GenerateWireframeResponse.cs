using System.Text.Json.Serialization;

namespace NLW.Shared.DTOs;

public sealed class GenerateWireframeResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("root")]
    public WireframeNodeDto Root { get; set; } = new();

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("mappings")]
    public List<ComponentMappingDto> Mappings { get; set; } = [];
}

public sealed class ComponentMappingDto
{
    [JsonPropertyName("blockLabel")]
    public string BlockLabel { get; set; } = string.Empty;

    [JsonPropertyName("blockType")]
    public string BlockType { get; set; } = string.Empty;

    [JsonPropertyName("componentName")]
    public string ComponentName { get; set; } = string.Empty;

    [JsonPropertyName("matchConfidence")]
    public string MatchConfidence { get; set; } = string.Empty;
}
