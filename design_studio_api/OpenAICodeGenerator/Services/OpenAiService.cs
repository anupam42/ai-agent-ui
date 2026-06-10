using System.Text;
using System.Text.Json;

using OpenAICodeGenerator.Services.Interfaces;

namespace OpenAICodeGenerator.Services;

public class OpenAiService(IHttpClientFactory httpFactory, IConfiguration config) : IOpenAiService
{
    public async Task<string> GenerateUiAsync(string userPrompt, string? imageBase64 = null)
    {
        var endpoint = config["AzureOpenAI:Endpoint"];
        var apiKey = config["AzureOpenAI:ApiKey"];
        var modelName = config["AzureOpenAI:Model"];

        var hasImage = !string.IsNullOrWhiteSpace(imageBase64);

        var http = httpFactory.CreateClient();
        http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var systemPrompt = $@"
            You are an AI that generates Angular 19 UI code.
            {(hasImage ? @"
            The user has uploaded a screenshot. RECREATE IT FAITHFULLY. Do not simplify.

            You MUST capture:
            - ALL toolbars, headers, and navigation bars (top + secondary if present)
            - EVERY visible icon (use Bootstrap Icons or similar, match position and color)
            - The EXACT color scheme (background, text, accents)
            - Collapsible/accordion sections with chevron icons if shown
            - User avatars, badges, notification counts
            - All buttons, even small icon-only ones
            - The exact spacing and visual hierarchy
            - Hover states and active states if implied by the UI

            Reproduce the FULL complexity. A simplified version is a FAILURE.
            " : "")}

            STRICT RULES:
            - Output ONLY valid JSON.
            - Do NOT include markdown, explanations, or comments.
            - Do NOT include ``` or code fences.
            - Only return JSON in this format:
            {{
              ""html"": ""..."",
              ""scss"": ""...""
            }}

            CONSTRAINTS:
            - HTML must be Angular 19 template syntax.
            - Use Bootstrap 5 classes for layout.
            - Use Bootstrap Icons (bi-*) for icons — include them as <i class=""bi bi-...""></i>.
            - No TypeScript.
            - SCSS must define exact colors matching the screenshot.
            ";

        object userContent;
        if (hasImage)
        {
            var imageUrl = imageBase64!.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
                ? imageBase64
                : $"data:image/png;base64,{imageBase64}";

            var promptText = string.IsNullOrWhiteSpace(userPrompt)
                ? "Recreate the UI shown in the attached image as accurately as possible."
                : userPrompt;

            userContent = new object[]
            {
                new { type = "text",      text = promptText },
                new { type = "image_url", image_url = new { url = imageUrl, detail = "high" } }
            };
        }
        else
        {
            userContent = userPrompt;
        }

        var requestBody = new
        {
            model = modelName,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user",   content = userContent }
            },
            max_completion_tokens = 8000
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await http.PostAsync(endpoint, content);

        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Azure OpenAI error: {responseJson}");

        using var doc = JsonDocument.Parse(responseJson);

        var result = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return result ?? "{}";
    }
}