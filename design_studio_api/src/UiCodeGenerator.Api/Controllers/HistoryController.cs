using Microsoft.AspNetCore.Mvc;

using UiCodeGenerator.Application.Abstractions;
using UiCodeGenerator.Application.DTOs;

namespace UiCodeGenerator.Api.Controllers;

[ApiController]
[Route("api/history")]
[Produces("application/json")]
public sealed class HistoryController(IHistoryService historyService) : ControllerBase
{
    /// <summary>Returns all saved history sessions, optionally filtered by a search term.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HistorySessionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HistorySessionResponse>>> GetAllAsync(
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var sessions = await historyService.GetAllAsync(search, ct);
        return Ok(sessions);
    }

    /// <summary>Saves (creates or updates) a design history session.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(HistorySessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HistorySessionResponse>> SaveAsync(
        [FromBody] SaveHistorySessionRequest request,
        CancellationToken ct)
    {
        var result = await historyService.SaveAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Deletes a single history session by id.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(string id, CancellationToken ct)
    {
        var deleted = await historyService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Clears all history sessions.</summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ClearAllAsync(CancellationToken ct)
    {
        await historyService.ClearAllAsync(ct);
        return NoContent();
    }
}
