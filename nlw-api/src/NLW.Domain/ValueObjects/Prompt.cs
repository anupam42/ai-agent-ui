namespace NLW.Domain.ValueObjects;

public sealed record Prompt
{
    public string Value { get; }

    private Prompt(string value) => Value = value;

    public static Prompt Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Prompt cannot be empty.", nameof(value));

        if (value.Length > 2000)
            throw new ArgumentException("Prompt cannot exceed 2000 characters.", nameof(value));

        return new Prompt(value.Trim());
    }

    public override string ToString() => Value;
}
