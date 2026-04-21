using MediatR;
using Microsoft.Extensions.Logging;
using NLW.Application.Common.Interfaces;
using NLW.Application.Common.Models;
using NLW.Domain.Entities;
using NLW.Domain.Enums;
using NLW.Domain.Exceptions;
using NLW.Shared.DTOs;

namespace NLW.Application.Features.Wireframe.Commands.GenerateWireframe;

public sealed class GenerateWireframeCommandHandler
    : IRequestHandler<GenerateWireframeCommand, Result<GenerateWireframeResponse>>
{
    private readonly IAIModelService _aiModelService;
    private readonly IPromptBuilderService _promptBuilder;
    private readonly IComponentIndexService _componentIndex;
    private readonly IWireframeRepository _repository;
    private readonly ILogger<GenerateWireframeCommandHandler> _logger;

    public GenerateWireframeCommandHandler(
        IAIModelService aiModelService,
        IPromptBuilderService promptBuilder,
        IComponentIndexService componentIndex,
        IWireframeRepository repository,
        ILogger<GenerateWireframeCommandHandler> logger)
    {
        _aiModelService = aiModelService;
        _promptBuilder = promptBuilder;
        _componentIndex = componentIndex;
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<GenerateWireframeResponse>> Handle(
        GenerateWireframeCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Generating wireframe for prompt: {Prompt}", request.Prompt[..Math.Min(50, request.Prompt.Length)]);

            var systemPrompt = await _promptBuilder.BuildWireframePromptAsync(request.Prompt, cancellationToken);
            var rootNode = await _aiModelService.GenerateWireframeAsync(request.Prompt, systemPrompt, cancellationToken);
            var mappings = BuildMappings(rootNode);
            var name = InferName(request.Prompt);

            var schema = WireframeSchema.Create(name, request.Prompt, MapToDomain(rootNode), mappings);
            await _repository.SaveAsync(schema, cancellationToken);

            _logger.LogInformation("Wireframe generated successfully: {Id}", schema.Id);

            return Result<GenerateWireframeResponse>.Success(MapToResponse(schema, rootNode));
        }
        catch (AIGenerationException ex)
        {
            _logger.LogWarning(ex, "AI generation failed for prompt");
            return Result<GenerateWireframeResponse>.Failure(ex.Code, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during wireframe generation");
            return Result<GenerateWireframeResponse>.Failure("UNEXPECTED_ERROR", "An unexpected error occurred.");
        }
    }

    private static string InferName(string prompt)
    {
        var p = prompt.ToLowerInvariant();
        if (p.Contains("dashboard") || p.Contains("admin")) return "Dashboard";
        if (p.Contains("login") || p.Contains("sign in") || p.Contains("auth")) return "Login Page";
        if (p.Contains("landing") || p.Contains("homepage")) return "Landing Page";
        if (p.Contains("register") || p.Contains("signup") || p.Contains("form")) return "Registration Form";
        if (p.Contains("profile") || p.Contains("account")) return "Profile Page";
        return "Page Layout";
    }

    private static List<ComponentMapping> BuildMappings(WireframeNodeDto node)
    {
        var result = new List<ComponentMapping>();
        void Walk(WireframeNodeDto n)
        {
            if (!string.IsNullOrWhiteSpace(n.MappedComponent))
            {
                result.Add(new ComponentMapping
                {
                    BlockLabel = n.Label ?? n.Type,
                    BlockType = Enum.TryParse<WireframeBlockType>(n.Type, true, out var bt) ? bt : WireframeBlockType.Card,
                    ComponentName = n.MappedComponent,
                    MatchConfidence = n.MappedComponent.StartsWith("Mat") ? MatchConfidence.Fallback : MatchConfidence.Exact
                });
            }
            n.Children?.ForEach(Walk);
        }
        Walk(node);
        return result;
    }

    private static NLW.Domain.ValueObjects.WireframeNode MapToDomain(WireframeNodeDto dto) =>
        new()
        {
            Id = dto.Id,
            Type = Enum.TryParse<WireframeBlockType>(dto.Type, true, out var t) ? t : WireframeBlockType.Card,
            Label = dto.Label,
            MappedComponent = dto.MappedComponent,
            Children = dto.Children?.Select(MapToDomain).ToList().AsReadOnly()
        };

    private static GenerateWireframeResponse MapToResponse(WireframeSchema schema, WireframeNodeDto root) =>
        new()
        {
            Id = schema.Id,
            Name = schema.Name,
            Prompt = schema.Prompt,
            Root = root,
            CreatedAt = schema.CreatedAt,
            Mappings = schema.Mappings.Select(m => new ComponentMappingDto
            {
                BlockLabel = m.BlockLabel,
                BlockType = m.BlockType.ToString(),
                ComponentName = m.ComponentName,
                MatchConfidence = m.MatchConfidence.ToString().ToLower()
            }).ToList()
        };
}
