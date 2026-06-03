namespace UiCodeGenerator.Application.Exceptions;

public sealed class AiProviderException(
    string message,
    int? providerStatusCode = null,
    string? providerErrorCode = null,
    Exception? innerException = null)
    : Exception(message, innerException)
{
    public int? ProviderStatusCode { get; } = providerStatusCode;

    public string? ProviderErrorCode { get; } = providerErrorCode;
}