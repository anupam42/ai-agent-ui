using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UiCodeGenerator.Application.Abstractions;
using UiCodeGenerator.Application.DTOs;
using UiCodeGenerator.Application.Exceptions;

namespace UiCodeGenerator.Infrastructure.DeepSeek;

public sealed class DeepSeekUiCodeGenerator(
    HttpClient httpClient,
    IOptionsMonitor<DeepSeekOptions> options,
    ILogger<DeepSeekUiCodeGenerator> logger) : IAiUiCodeGenerator
{
    private static readonly JsonSerializerOptions DeepSeekJsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions ModelPayloadJsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public async Task<AiUiGenerationPayload> GenerateAsync(GenerateUiRequest request, CancellationToken cancellationToken)
    {
        var currentOptions = options.CurrentValue;
        if (string.IsNullOrWhiteSpace(currentOptions.ApiKey))
        {
            throw new AiConfigurationException("DeepSeek:ApiKey is not configured. Set it with user secrets or the DeepSeek__ApiKey environment variable.");
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", currentOptions.ApiKey);
        httpRequest.Content = JsonContent.Create(CreateChatRequest(request, currentOptions), options: DeepSeekJsonOptions);

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            ThrowProviderException(response.StatusCode, responseText);
        }

        var chatResponse = DeserializeDeepSeekResponse(responseText);
        var content = chatResponse.Choices.FirstOrDefault()?.Message.Content;

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new AiProviderException("DeepSeek returned an empty chat completion.", (int)response.StatusCode);
        }

        var payload = DeserializeModelPayload(content);
        NormalizeAndValidatePayload(payload, request);

        logger.LogInformation("Generated UI code with DeepSeek model {Model}.", currentOptions.Model);
        return payload;
    }

    private static DeepSeekChatRequest CreateChatRequest(GenerateUiRequest request, DeepSeekOptions options)
    {
        return new DeepSeekChatRequest
        {
            Model = options.Model,
            Messages =
            [
                new DeepSeekMessage("system", BuildSystemPrompt()),
                new DeepSeekMessage("user", BuildUserPrompt(request))
            ],
            Thinking = new DeepSeekThinking(options.EnableThinking ? "enabled" : "disabled"),
            ReasoningEffort = options.EnableThinking ? options.ReasoningEffort.ToLowerInvariant() : null,
            MaxTokens = options.MaxTokens,
            Temperature = options.Temperature,
            Stream = false,
            ResponseFormat = new DeepSeekResponseFormat("json_object")
        };
    }

    private static string BuildSystemPrompt()
    {
        return """
            You are a senior frontend engineer that generates production-ready UI screens.
            Return only one valid JSON object. Do not wrap it in markdown fences. Do not include commentary outside JSON.

            The JSON object must match this shape:
            {
              "preview": {
                "title": "string",
                "description": "string",
                "layout": "string",
                "theme": {
                  "primaryColor": "#2563eb",
                  "backgroundColor": "#f8fafc",
                  "textColor": "#0f172a",
                  "fontFamily": "Inter, sans-serif"
                },
                "components": [
                  {
                    "id": "string",
                    "type": "screen | section | card | form | input | button | text | image | navigation | list | grid",
                    "label": "string or null",
                    "text": "string or null",
                    "props": {},
                    "children": []
                  }
                ]
              },
              "sourceCode": {
                "language": "tsx | html | vue | svelte",
                "framework": "react | html | vue | angular | svelte",
                "files": [
                  {
                    "path": "GeneratedScreen.tsx",
                    "content": "source code"
                  }
                ]
              },
              "notes": ["short implementation note"]
            }

            Requirements:
            - Produce complete, copyable source code.
            - Keep code self-contained unless the requested framework normally imports core framework APIs.
            - Use semantic HTML and accessible labels.
            - Make the preview tree useful for a canvas renderer.
            - Ensure every component id is stable, lowercase, and unique.
            """;
    }

    private static string BuildUserPrompt(GenerateUiRequest request)
    {
        return $$"""
            User prompt:
            {{request.Prompt.Trim()}}

            Generation preferences:
            - Framework: {{request.Framework.Trim().ToLowerInvariant()}}
            - Styling: {{request.Styling.Trim().ToLowerInvariant()}}
            - Component name: {{request.ComponentName.Trim()}}
            - Responsive layout required: {{request.IncludeResponsiveLayout}}
            - Accessibility required: {{request.IncludeAccessibility}}

            Return JSON with preview data for canvas rendering and source code for a generated code panel.
            """;
    }

    private static DeepSeekChatResponse DeserializeDeepSeekResponse(string responseText)
    {
        try
        {
            return JsonSerializer.Deserialize<DeepSeekChatResponse>(responseText, DeepSeekJsonOptions)
                ?? throw new AiProviderException("DeepSeek returned an empty response body.");
        }
        catch (JsonException exception)
        {
            throw new AiProviderException("DeepSeek returned a response that could not be parsed.", innerException: exception);
        }
    }

    private static AiUiGenerationPayload DeserializeModelPayload(string content)
    {
        try
        {
            return JsonSerializer.Deserialize<AiUiGenerationPayload>(content, ModelPayloadJsonOptions)
                ?? throw new AiProviderException("DeepSeek returned empty UI generation JSON.");
        }
        catch (JsonException exception)
        {
            throw new AiProviderException("DeepSeek returned invalid UI generation JSON.", innerException: exception);
        }
    }

    private static void NormalizeAndValidatePayload(AiUiGenerationPayload payload, GenerateUiRequest request)
    {
        payload.Preview ??= new();
        payload.Preview.Theme ??= new();
        payload.Preview.Components ??= [];
        payload.SourceCode ??= new();
        payload.SourceCode.Files ??= [];
        payload.Notes ??= [];

        if (string.IsNullOrWhiteSpace(payload.Preview.Title))
        {
            payload.Preview.Title = request.ComponentName;
        }

        if (string.IsNullOrWhiteSpace(payload.Preview.Description))
        {
            payload.Preview.Description = request.Prompt.Trim();
        }

        if (string.IsNullOrWhiteSpace(payload.SourceCode.Framework))
        {
            payload.SourceCode.Framework = request.Framework.Trim().ToLowerInvariant();
        }

        if (string.IsNullOrWhiteSpace(payload.SourceCode.Language))
        {
            payload.SourceCode.Language = request.Framework.Equals("react", StringComparison.OrdinalIgnoreCase) ? "tsx" : "html";
        }

        if (payload.SourceCode.Files.Count == 0 || payload.SourceCode.Files.Any(file => string.IsNullOrWhiteSpace(file.Content)))
        {
            throw new AiProviderException("DeepSeek did not return any usable source code files.");
        }
    }

    private static void ThrowProviderException(HttpStatusCode statusCode, string responseText)
    {
        var providerStatusCode = (int)statusCode;
        var providerError = TryReadProviderError(responseText);
        var message = providerError?.Message ?? $"DeepSeek API returned HTTP {providerStatusCode}.";

        throw new AiProviderException(message, providerStatusCode, providerError?.Code);
    }

    private static DeepSeekError? TryReadProviderError(string responseText)
    {
        if (string.IsNullOrWhiteSpace(responseText))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<DeepSeekErrorEnvelope>(responseText, DeepSeekJsonOptions)?.Error;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed class DeepSeekChatRequest
    {
        public string Model { get; init; } = string.Empty;

        public List<DeepSeekMessage> Messages { get; init; } = [];

        public DeepSeekThinking? Thinking { get; init; }

        public string? ReasoningEffort { get; init; }

        public int MaxTokens { get; init; }

        public double Temperature { get; init; }

        public bool Stream { get; init; }

        public DeepSeekResponseFormat ResponseFormat { get; init; } = new("json_object");
    }

    private sealed record DeepSeekMessage(string Role, string Content);

    private sealed record DeepSeekThinking(string Type);

    private sealed record DeepSeekResponseFormat(string Type);

    private sealed class DeepSeekChatResponse
    {
        public List<DeepSeekChoice> Choices { get; init; } = [];
    }

    private sealed class DeepSeekChoice
    {
        public DeepSeekAssistantMessage Message { get; init; } = new();
    }

    private sealed class DeepSeekAssistantMessage
    {
        public string? Content { get; init; }
    }

    private sealed class DeepSeekErrorEnvelope
    {
        public DeepSeekError? Error { get; init; }
    }

    private sealed class DeepSeekError
    {
        public string? Message { get; init; }

        public string? Code { get; init; }
    }
}
