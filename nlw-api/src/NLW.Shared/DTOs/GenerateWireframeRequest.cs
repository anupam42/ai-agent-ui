using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NLW.Shared.DTOs;

public sealed class GenerateWireframeRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(2000)]
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;
}
