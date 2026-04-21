using MediatR;
using Microsoft.AspNetCore.Mvc;
using NLW.Application.Features.Wireframe.Commands.GenerateWireframe;
using NLW.Application.Features.Wireframe.Queries.GetWireframeHistory;
using NLW.Shared.DTOs;
using System.Runtime.CompilerServices;

namespace NLW.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class WireframeController : ControllerBase
{
    private readonly IMediator _mediator;

    public WireframeController(IMediator mediator) => _mediator = mediator;

    /// <summary>Generate a wireframe from a natural language prompt.</summary>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(GenerateWireframeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Generate(
        [FromBody] GenerateWireframeRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateWireframeCommand(request.Prompt), ct);

        return result.Match<IActionResult>(
            onSuccess: Ok,
            onFailure: (code, msg) => code switch
            {
                "AI_GENERATION_FAILED" => StatusCode(502, Problem(msg, statusCode: 502)),
                _ => BadRequest(Problem(msg, statusCode: 400))
            });
    }

    /// <summary>Stream wireframe generation via Server-Sent Events.</summary>
    [HttpGet("stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async IAsyncEnumerable<WireframeNodeDto> Stream(
        [FromQuery] string prompt,
        [EnumeratorCancellation] CancellationToken ct)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no");

        var command = new GenerateWireframeCommand(prompt);
        // Streaming is handled by infrastructure — yield results as they arrive
        await foreach (var node in StreamFromCommand(command, ct))
            yield return node;
    }

    /// <summary>Get recent wireframe generation history.</summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(IReadOnlyList<GenerateWireframeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory([FromQuery] int count = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetWireframeHistoryQuery(count), ct);
        return result.Match<IActionResult>(Ok, (_, msg) => BadRequest(Problem(msg)));
    }

    private async IAsyncEnumerable<WireframeNodeDto> StreamFromCommand(
        GenerateWireframeCommand command,
        [EnumeratorCancellation] CancellationToken ct)
    {
        // Placeholder — real streaming wired through IAIModelService.StreamWireframeAsync
        var result = await _mediator.Send(command, ct);
        if (result.IsSuccess && result.Value is not null)
            yield return result.Value.Root;
    }
}
