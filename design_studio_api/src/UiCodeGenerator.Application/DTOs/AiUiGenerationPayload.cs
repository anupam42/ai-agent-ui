using UiCodeGenerator.Domain.Models;

namespace UiCodeGenerator.Application.DTOs;

public sealed class AiUiGenerationPayload
{
    public UiPreview Preview { get; set; } = new();

    public GeneratedSourceCode SourceCode { get; set; } = new();

    public List<string> Notes { get; set; } = [];
}
