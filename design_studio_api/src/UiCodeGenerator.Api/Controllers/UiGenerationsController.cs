using Microsoft.AspNetCore.Mvc;
using UiCodeGenerator.Application.Abstractions;
using UiCodeGenerator.Application.DTOs;
using UiCodeGenerator.Domain.Models;

namespace UiCodeGenerator.Api.Controllers;

[ApiController]
[Route("api/ui-generations")]
[Produces("application/json")]
public sealed class UiGenerationsController(IUiGenerationService uiGenerationService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(GenerateUiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<GenerateUiResponse>> GenerateAsync(
        [FromBody] GenerateUiRequest request,
        CancellationToken cancellationToken)
    {
        var response = await uiGenerationService.GenerateAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("example")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public ActionResult<object> GetExample()
    {
        return Ok(new
        {
            request = CreateExampleRequest(),
            response = CreateExampleResponse()
        });
    }

    private static GenerateUiRequest CreateExampleRequest()
    {
        return new GenerateUiRequest
        {
            Prompt = "Create a responsive SaaS login screen with email, password, forgot password link, and a primary sign in action.",
            Framework = "react",
            Styling = "tailwind",
            ComponentName = "LoginScreen",
            IncludeResponsiveLayout = true,
            IncludeAccessibility = true
        };
    }

    private static GenerateUiResponse CreateExampleResponse()
    {
        return new GenerateUiResponse(
            Guid.Parse("2b66db8c-78b0-4a65-88a8-c35b96303f44"),
            DateTimeOffset.Parse("2026-05-17T16:45:00Z"),
            "Create a responsive SaaS login screen with email, password, forgot password link, and a primary sign in action.",
            "react",
            new UiPreview
            {
                Title = "SaaS Login Screen",
                Description = "Centered authentication screen with email and password controls.",
                Layout = "centered-auth-card",
                Theme = new UiTheme
                {
                    PrimaryColor = "#2563eb",
                    BackgroundColor = "#f8fafc",
                    TextColor = "#0f172a",
                    FontFamily = "Inter, sans-serif"
                },
                Components =
                [
                    new UiPreviewComponent
                    {
                        Id = "auth-screen",
                        Type = "screen",
                        Props = new Dictionary<string, object?>
                        {
                            ["maxWidth"] = "420px"
                        },
                        Children =
                        [
                            new UiPreviewComponent
                            {
                                Id = "email-input",
                                Type = "input",
                                Label = "Email address",
                                Props = new Dictionary<string, object?>
                                {
                                    ["inputType"] = "email",
                                    ["placeholder"] = "you@example.com"
                                }
                            },
                            new UiPreviewComponent
                            {
                                Id = "submit-button",
                                Type = "button",
                                Text = "Sign in",
                                Props = new Dictionary<string, object?>
                                {
                                    ["variant"] = "primary"
                                }
                            }
                        ]
                    }
                ]
            },
            new GeneratedSourceCode
            {
                Language = "tsx",
                Framework = "react",
                Files =
                [
                    new GeneratedSourceFile
                    {
                        Path = "LoginScreen.tsx",
                        Content = "export default function LoginScreen() { return <main>Sign in</main>; }"
                    }
                ]
            },
            ["Example response only; POST /api/ui-generations calls DeepSeek."]);
    }
}
