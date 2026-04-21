using MediatR;
using NLW.Application.Common.Models;
using NLW.Shared.DTOs;

namespace NLW.Application.Features.Wireframe.Commands.GenerateWireframe;

public sealed record GenerateWireframeCommand(string Prompt)
    : IRequest<Result<GenerateWireframeResponse>>;
