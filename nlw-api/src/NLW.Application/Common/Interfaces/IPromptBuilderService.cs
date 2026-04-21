namespace NLW.Application.Common.Interfaces;

public interface IPromptBuilderService
{
    Task<string> BuildWireframePromptAsync(string userPrompt, CancellationToken ct = default);
}
