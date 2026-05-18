namespace UiCodeGenerator.Application.Exceptions;

public sealed class AiProviderException : Exception
{
    public AiProviderException(string message, int? providerStatusCode = null, string? providerErrorCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ProviderStatusCode = providerStatusCode;
        ProviderErrorCode = providerErrorCode;
    }

    public int? ProviderStatusCode { get; }

    public string? ProviderErrorCode { get; }
}
