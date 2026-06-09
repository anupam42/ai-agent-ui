using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using OpenAICodeGenerator.Models;
using OpenAICodeGenerator.Services.Interfaces;

namespace OpenAICodeGenerator.Controllers;

[ApiController]
[Route("api/ui-generation")]
public class UiGenerationController(IOpenAiService openAiService) : ControllerBase
{
    public static JsonSerializerOptions JsonSerializerOptions => new()
    {
        PropertyNameCaseInsensitive = true
    };

    [HttpPost]
    public async Task<IActionResult> GenerateUi(
        [FromBody] UiRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest("Prompt is required");

        var rawResponse = await openAiService.GenerateUiAsync(request.Prompt);

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