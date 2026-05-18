using UiCodeGenerator.Domain.Models;

namespace UiCodeGenerator.Application.DTOs;

public sealed record GenerateUiResponse(
    Guid Id,
    DateTimeOffset GeneratedAtUtc,
    string Prompt,
    string Framework,
    UiPreview Preview,
    GeneratedSourceCode SourceCode,
    IReadOnlyList<string> Notes);
