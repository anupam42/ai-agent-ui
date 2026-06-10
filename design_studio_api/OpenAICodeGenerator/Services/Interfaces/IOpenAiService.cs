namespace OpenAICodeGenerator.Services.Interfaces;

public interface IOpenAiService
{
    Task<string> GenerateUiAsync(string userPrompt, string? imageBase64 = null);
}