using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using OpenAICodeGenerator.Models;
using OpenAICodeGenerator.Services.Interfaces;

namespace OpenAICodeGenerator.Controllers;

[ApiController]
[Route("api/ui-generation")]
public class UiGenerationController(IOpenAiService openAiService) : ControllerBase
{
    public JsonSerializerOptions JsonSerializerOptions => new()
    {
        PropertyNameCaseInsensitive = true
    };

    [HttpPost]
    public async Task<IActionResult> GenerateUi(
        [FromBody] UiRequestDto request)
    {
        // Need at least a prompt OR an image.
        if (string.IsNullOrWhiteSpace(request.Prompt) && !request.HasImage)
            return BadRequest("Either a Prompt or an ImageBase64 must be provided.");

        var rawResponse = await openAiService.GenerateUiAsync(
            request.Prompt,
            request.ImageBase64);

        try
        {
            var parsed = JsonSerializer.Deserialize<UiResponseDto>(
                rawResponse,
                JsonSerializerOptions);

            return parsed == null
                ? throw new Exception("Invalid AI response")
                : (IActionResult)Ok(parsed);
        }
        catch
        {
            return BadRequest(new
            {
                error = "Invalid AI response format",
                rawResponse
            });
        }
    }
}