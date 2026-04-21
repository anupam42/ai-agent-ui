using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLW.Application.Common.Interfaces;
using NLW.Domain.Exceptions;
using NLW.Infrastructure.Configuration;
using NLW.Shared.DTOs;

namespace NLW.Infrastructure.AI;

public sealed class ClaudeAIService : IAIModelService
{
    private readonly HttpClient _http;
    private readonly AISettings _settings;
    private readonly ILogger<ClaudeAIService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ClaudeAIService(HttpClient http, IOptions<AISettings> settings, ILogger<ClaudeAIService> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<WireframeNodeDto> GenerateWireframeAsync(
        string prompt, string systemPrompt, CancellationToken ct = default)
    {
        var requestBody = new ClaudeRequest
        {
            Model = _settings.Model,
            MaxTokens = _settings.MaxTokens,
            System = systemPrompt,
            Messages = [new ClaudeMessage { Role = "user", Content = prompt }]
        };

        var json = JsonSerializer.Serialize(requestBody, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);
        _http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _http.PostAsync(_settings.BaseUrl + "/v1/messages", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Claude API error {Status}: {Body}", response.StatusCode, err);
            throw new AIGenerationException($"AI model returned {response.StatusCode}.");
        }

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseJson, JsonOptions)
            ?? throw new AIGenerationException("Empty response from AI model.");

        var rawText = claudeResponse.Content?.FirstOrDefault()?.Text
            ?? throw new AIGenerationException("No text content in AI response.");

        return ParseWireframeNode(rawText);
    }

    public async IAsyncEnumerable<WireframeNodeDto> StreamWireframeAsync(
        string prompt, string systemPrompt, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var requestBody = new ClaudeRequest
        {
            Model = _settings.Model,
            MaxTokens = _settings.MaxTokens,
            System = systemPrompt,
            Stream = true,
            Messages = [new ClaudeMessage { Role = "user", Content = prompt }]
        };

        var json = JsonSerializer.Serialize(requestBody, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);
        _http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        using var request = new HttpRequestMessage(HttpMethod.Post, _settings.BaseUrl + "/v1/messages") { Content = content };
        using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);
        var buffer = new StringBuilder();

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrEmpty(line) || !line.StartsWith("data:")) continue;

            var data = line["data: ".Length..].Trim();
            if (data == "[DONE]") break;

            var evt = JsonSerializer.Deserialize<ClaudeStreamEvent>(data, JsonOptions);
            if (evt?.Type == "content_block_delta" && evt.Delta?.Text is { } text)
            {
                buffer.Append(text);
                if (TryParsePartial(buffer.ToString(), out var partial))
                    yield return partial!;
            }
        }

        if (buffer.Length > 0)
            yield return ParseWireframeNode(buffer.ToString());
    }

    private static WireframeNodeDto ParseWireframeNode(string raw)
    {
        var cleaned = raw.Trim();
        if (cleaned.StartsWith("```")) cleaned = cleaned.Split('\n').Skip(1).TakeWhile(l => !l.StartsWith("```")).Aggregate((a, b) => a + "\n" + b);

        return JsonSerializer.Deserialize<WireframeNodeDto>(cleaned, JsonOptions)
            ?? throw new AIGenerationException("Could not parse wireframe JSON from AI response.");
    }

    private static bool TryParsePartial(string raw, out WireframeNodeDto? node)
    {
        node = null;
        try { node = JsonSerializer.Deserialize<WireframeNodeDto>(raw); return node is not null; }
        catch { return false; }
    }

    private sealed record ClaudeRequest
    {
        [JsonPropertyName("model")] public string Model { get; init; } = string.Empty;
        [JsonPropertyName("max_tokens")] public int MaxTokens { get; init; }
        [JsonPropertyName("system")] public string? System { get; init; }
        [JsonPropertyName("stream")] public bool? Stream { get; init; }
        [JsonPropertyName("messages")] public ClaudeMessage[] Messages { get; init; } = [];
    }

    private sealed record ClaudeMessage
    {
        [JsonPropertyName("role")] public string Role { get; init; } = string.Empty;
        [JsonPropertyName("content")] public string Content { get; init; } = string.Empty;
    }

    private sealed record ClaudeResponse
    {
        [JsonPropertyName("content")] public ClaudeContentBlock[]? Content { get; init; }
    }

    private sealed record ClaudeContentBlock
    {
        [JsonPropertyName("text")] public string? Text { get; init; }
    }

    private sealed record ClaudeStreamEvent
    {
        [JsonPropertyName("type")] public string? Type { get; init; }
        [JsonPropertyName("delta")] public ClaudeDelta? Delta { get; init; }
    }

    private sealed record ClaudeDelta
    {
        [JsonPropertyName("text")] public string? Text { get; init; }
    }
}
