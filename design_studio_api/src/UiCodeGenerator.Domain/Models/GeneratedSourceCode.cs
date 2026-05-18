namespace UiCodeGenerator.Domain.Models;

public sealed class GeneratedSourceCode
{
    public string Language { get; set; } = "tsx";

    public string Framework { get; set; } = "react";

    public List<GeneratedSourceFile> Files { get; set; } = [];
}
