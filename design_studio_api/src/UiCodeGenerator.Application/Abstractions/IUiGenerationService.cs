using UiCodeGenerator.Application.DTOs;

namespace UiCodeGenerator.Application.Abstractions;

public interface IUiGenerationService
{
    Task<GenerateUiResponse> GenerateAsync(GenerateUiRequest request, CancellationToken cancellationToken);
}