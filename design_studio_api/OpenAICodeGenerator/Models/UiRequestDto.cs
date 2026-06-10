namespace OpenAICodeGenerator.Models;

public class UiRequestDto
{
    public string Prompt { get; set; } = string.Empty;

    public string? ImageBase64 { get; set; }

    public bool HasImage =>
        !string.IsNullOrWhiteSpace(ImageBase64);
}