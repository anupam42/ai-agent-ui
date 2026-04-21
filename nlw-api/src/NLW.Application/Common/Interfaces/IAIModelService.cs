using NLW.Shared.DTOs;

namespace NLW.Application.Common.Interfaces;

public interface IAIModelService
{
    Task<WireframeNodeDto> GenerateWireframeAsync(string prompt, string systemPrompt, CancellationToken ct = default);
    IAsyncEnumerable<WireframeNodeDto> StreamWireframeAsync(string prompt, string systemPrompt, CancellationToken ct = default);
}
