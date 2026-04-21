using System.Text.Json.Serialization;

namespace NLW.Shared.DTOs;

public sealed class WireframeNodeDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("children")]
    public List<WireframeNodeDto>? Children { get; set; }

    [JsonPropertyName("span")]
    public int? Span { get; set; }

    [JsonPropertyName("width")]
    public string? Width { get; set; }

    [JsonPropertyName("height")]
    public string? Height { get; set; }

    [JsonPropertyName("sticky")]
    public bool? Sticky { get; set; }

    [JsonPropertyName("items")]
    public List<string>? Items { get; set; }

    [JsonPropertyName("columns")]
    public List<string>? Columns { get; set; }

    [JsonPropertyName("rows")]
    public int? Rows { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("mappedComponent")]
    public string? MappedComponent { get; set; }

    [JsonPropertyName("placeholder")]
    public string? Placeholder { get; set; }
}
