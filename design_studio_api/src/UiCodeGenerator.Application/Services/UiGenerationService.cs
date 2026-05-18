using UiCodeGenerator.Application.Abstractions;
using UiCodeGenerator.Application.DTOs;

namespace UiCodeGenerator.Application.Services;

public sealed class UiGenerationService(IAiUiCodeGenerator aiUiCodeGenerator) : IUiGenerationService
{
    public async Task<GenerateUiResponse> GenerateAsync(GenerateUiRequest request, CancellationToken cancellationToken)
    {
        var aiPayload = await aiUiCodeGenerator.GenerateAsync(request, cancellationToken);

        return new GenerateUiResponse(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            request.Prompt.Trim(),
            request.Framework.Trim().ToLowerInvariant(),
            aiPayload.Preview,
            aiPayload.SourceCode,
            aiPayload.Notes);
    }
}
