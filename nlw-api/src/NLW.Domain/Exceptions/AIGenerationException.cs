namespace NLW.Domain.Exceptions;

public sealed class AIGenerationException : DomainException
{
    public AIGenerationException(string message)
        : base("AI_GENERATION_FAILED", message) { }

    public AIGenerationException(string message, Exception inner)
        : base("AI_GENERATION_FAILED", message)
    {
    }
}
