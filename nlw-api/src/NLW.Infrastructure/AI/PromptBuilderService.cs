using NLW.Application.Common.Interfaces;
using NLW.Shared.Constants;

namespace NLW.Infrastructure.AI;

public sealed class PromptBuilderService : IPromptBuilderService
{
    private readonly IComponentIndexService _componentIndex;

    public PromptBuilderService(IComponentIndexService componentIndex)
        => _componentIndex = componentIndex;

    public async Task<string> BuildWireframePromptAsync(string userPrompt, CancellationToken ct = default)
    {
        var relevantComponents = await _componentIndex.SearchAsync(userPrompt, topN: 12, ct);

        var componentContext = relevantComponents.Count > 0
            ? $"\n\nAVAILABLE COMPANY COMPONENTS (prefer these over Material fallbacks):\n" +
              string.Join("\n", relevantComponents.Select(c =>
                  $"- selector: {c.Selector}, inputs: [{string.Join(", ", c.Inputs)}], maps to block: {c.MappedBlockType}, description: {c.Description}"))
            : string.Empty;

        return AIPromptConstants.SystemPrompt + componentContext;
    }
}
