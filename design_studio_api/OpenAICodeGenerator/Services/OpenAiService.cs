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
You are an expert Angular 19 UI generator.

STRICT OUTPUT RULES:
- Return ONLY valid JSON.
- Do NOT return markdown.
- Do NOT return explanations.
- Do NOT return comments.
- Do NOT return code fences.
- Do NOT return any text before or after the JSON.
- The response MUST be a single JSON object.
- Escape all quotes, backslashes, and newlines correctly for JSON strings.

REQUIRED JSON FORMAT:
{
  ""html"": ""<Angular 19 template>"",
  ""scss"": ""<SCSS styles>""
}

ANGULAR RULES:
- Generate ONLY Angular 19 template HTML and SCSS.
- Do NOT generate TypeScript.
- Do NOT generate component code.
- Do NOT generate module code.
- Do NOT generate imports.
- Use Angular 19 template syntax where applicable.
- Use @if, @for, and @switch when control flow is needed.
- Do NOT include HTML comments.
- Do NOT include SCSS comments.

UI/UX RULES:
- Create stunning, ultra-modern, premium UI/UX design with exceptional visual hierarchy and world-class aesthetics. Use clean, minimalist layout, perfect spacing, elegant typography, glassmorphism effects, subtle gradients, smooth shadows, modern cards, and beautiful micro-interactions. The design should feel futuristic, luxurious, and highly professional while maintaining excellent usability and accessibility.
- Prioritize excellent visual hierarchy, spacing, responsiveness, and accessibility.
- Use Bootstrap 5 (with SCSS) or Angular Material classes (with SCSS), as requested by the user, to create a modern, visually appealing, responsive, and accessible user interface.
- If the user does not specify a UI framework, choose the most appropriate option between SCSS, Bootstrap 5 and Angular Material, and create a modern, visually appealing, responsive, and accessible user interface.
- Ensure the layout is fully responsive.
- Follow modern design patterns and best practices.
- Use attractive cards, sections, typography, buttons, and spacing.
- Generate realistic sample content when needed.

SCSS RULES:
- Keep SCSS clean and production-ready.
- Avoid unnecessary styles.
- Use modern CSS/SCSS techniques.
- Ensure styles complement the generated layout.

VALIDATION:
- The response must be parseable JSON.
- Both ""html"" and ""scss"" properties are required.
- Never return null values.
- Never return additional properties.
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