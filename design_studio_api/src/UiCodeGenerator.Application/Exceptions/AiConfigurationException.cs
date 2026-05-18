namespace UiCodeGenerator.Application.Exceptions;

public sealed class AiConfigurationException : Exception
{
    public AiConfigurationException(string message)
        : base(message)
    {
    }
}
