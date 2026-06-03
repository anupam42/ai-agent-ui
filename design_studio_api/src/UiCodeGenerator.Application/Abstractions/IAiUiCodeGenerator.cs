using UiCodeGenerator.Application.DTOs;

namespace UiCodeGenerator.Application.Abstractions;

public interface IAiUiCodeGenerator
{
    Task<AiUiGenerationPayload> GenerateAsync(GenerateUiRequest request, CancellationToken cancellationToken);
}