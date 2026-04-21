using MediatR;
using NLW.Application.Common.Models;
using NLW.Shared.DTOs;

namespace NLW.Application.Features.Wireframe.Queries.GetWireframeHistory;

public sealed record GetWireframeHistoryQuery(int Count = 20)
    : IRequest<Result<IReadOnlyList<GenerateWireframeResponse>>>;
