using MediatR;
using NLW.Application.Common.Interfaces;
using NLW.Application.Common.Models;
using NLW.Shared.DTOs;

namespace NLW.Application.Features.Wireframe.Queries.GetWireframeHistory;

public sealed class GetWireframeHistoryQueryHandler
    : IRequestHandler<GetWireframeHistoryQuery, Result<IReadOnlyList<GenerateWireframeResponse>>>
{
    private readonly IWireframeRepository _repository;

    public GetWireframeHistoryQueryHandler(IWireframeRepository repository)
        => _repository = repository;

    public async Task<Result<IReadOnlyList<GenerateWireframeResponse>>> Handle(
        GetWireframeHistoryQuery request, CancellationToken cancellationToken)
    {
        var schemas = await _repository.GetRecentAsync(request.Count, cancellationToken);

        var responses = schemas.Select(s => new GenerateWireframeResponse
        {
            Id = s.Id,
            Name = s.Name,
            Prompt = s.Prompt,
            CreatedAt = s.CreatedAt,
            Root = new WireframeNodeDto { Type = s.Root.Type.ToString() },
            Mappings = s.Mappings.Select(m => new ComponentMappingDto
            {
                BlockLabel = m.BlockLabel,
                BlockType = m.BlockType.ToString(),
                ComponentName = m.ComponentName,
                MatchConfidence = m.MatchConfidence.ToString().ToLower()
            }).ToList()
        }).ToList().AsReadOnly();

        return Result<IReadOnlyList<GenerateWireframeResponse>>.Success(responses);
    }
}
