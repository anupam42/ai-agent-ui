using System.Text;
using System.Text.Json;

using OpenAICodeGenerator.Services.Interfaces;

namespace OpenAICodeGenerator.Services;

public class OpenAiService(IHttpClientFactory httpFactory, IConfiguration config) : IOpenAiService
{
    public async Task<string> GenerateUiAsync(string userPrompt)
    {
        var endpoint = config["AzureOpenAI:Endpoint"];
        var apiKey = config["AzureOpenAI:ApiKey"];
        var modelName = config["AzureOpenAI:Model"];

        var http = httpFactory.CreateClient();

        http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var systemPrompt = @"
You are an AI that generates Angular 19 UI code.

STRICT RULES:
- Output ONLY valid JSON.
- Do NOT include markdown, explanations, or comments.
- Do NOT include ``` or code fences.
- Only return JSON in this format:
{
  ""html"": ""..."",
  ""scss"": ""...""
}

CONSTRAINTS:
- HTML must be Angular 19 template syntax.
- Use Bootstrap 5 classes for layout and responsiveness.
- No TypeScript.
- No Angular component code.
- No comments inside HTML or SCSS.
- SCSS must be clean and minimal.
";

        var requestBody = new
        {
            model = modelName,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = systemPrompt
                },
                new
                {
                    role = "user",
                    content = userPrompt
                }
            },

            // Optional for Agentic AI
            tools = Array.Empty<object>(),
            tool_choice = "auto",

            temperature = 0.3,
            max_tokens = 2000
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