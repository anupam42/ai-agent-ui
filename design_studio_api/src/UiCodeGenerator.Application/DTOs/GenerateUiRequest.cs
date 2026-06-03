using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UiCodeGenerator.Application.DTOs;

public sealed partial class GenerateUiRequest : IValidatableObject
{
    // Removed: react, html, vue, svelte
    private static readonly HashSet<string> AllowedFrameworks = new(StringComparer.OrdinalIgnoreCase)
    {
        "angular"
    };

    // Removed: tailwind, chakra-ui
    private static readonly HashSet<string> AllowedStyling = new(StringComparer.OrdinalIgnoreCase)
    {
        "css",
        "scss",
        "bootstrap",
        "material-ui"
    };

    private static readonly Regex ComponentNamePattern = GetRegex();

    [Required]
    [StringLength(4000, MinimumLength = 10)]
    public string Prompt { get; init; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Framework { get; init; } = "angular";

    [Required]
    [StringLength(40)]
    public string Styling { get; init; } = "bootstrap";

    [Required]
    [StringLength(60, MinimumLength = 2)]
    public string ComponentName { get; init; } = "GeneratedScreen";

    public bool IncludeResponsiveLayout { get; init; } = true;

    public bool IncludeAccessibility { get; init; } = true;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (!AllowedFrameworks.Contains(Framework))
            yield return new ValidationResult(
                $"Framework must be one of: {string.Join(", ", AllowedFrameworks.Order())}.",
                [nameof(Framework)]);

        if (!AllowedStyling.Contains(Styling))
            yield return new ValidationResult(
                $"Styling must be one of: {string.Join(", ", AllowedStyling.Order())}.",
                [nameof(Styling)]);

        if (!ComponentNamePattern.IsMatch(ComponentName))
            yield return new ValidationResult(
                "ComponentName must be PascalCase, start with an uppercase letter, and contain only letters or numbers.",
                [nameof(ComponentName)]);
    }

    [GeneratedRegex("^[A-Z][A-Za-z0-9]{1,60}$", RegexOptions.Compiled)]
    public static partial Regex GetRegex();
}