namespace UiCodeGenerator.Domain.Models;

public sealed class GeneratedSourceCode
{
    public string Language { get; set; } = "html";

    public string Framework { get; set; } = "angular";

    public List<GeneratedSourceFile> Files { get; set; } = [];
}