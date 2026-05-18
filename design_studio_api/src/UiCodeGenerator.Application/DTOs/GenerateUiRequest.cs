using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UiCodeGenerator.Application.DTOs;

public sealed class GenerateUiRequest : IValidatableObject
{
    private static readonly HashSet<string> AllowedFrameworks = new(StringComparer.OrdinalIgnoreCase)
    {
        "react",
        "html",
        "vue",
        "angular",
        "svelte"
    };

    private static readonly HashSet<string> AllowedStyling = new(StringComparer.OrdinalIgnoreCase)
    {
        "tailwind",
        "css",
        "scss",
        "bootstrap",
        "material-ui",
        "chakra-ui"
    };

    private static readonly Regex ComponentNamePattern = new("^[A-Z][A-Za-z0-9]{1,60}$", RegexOptions.Compiled);

    [Required]
    [StringLength(4000, MinimumLength = 10)]
    public string Prompt { get; init; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Framework { get; init; } = "react";

    [Required]
    [StringLength(40)]
    public string Styling { get; init; } = "tailwind";

    [Required]
    [StringLength(60, MinimumLength = 2)]
    public string ComponentName { get; init; } = "GeneratedScreen";

    public bool IncludeResponsiveLayout { get; init; } = true;

    public bool IncludeAccessibility { get; init; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!AllowedFrameworks.Contains(Framework))
        {
            yield return new ValidationResult(
                $"Framework must be one of: {string.Join(", ", AllowedFrameworks.Order())}.",
                [nameof(Framework)]);
        }

        if (!AllowedStyling.Contains(Styling))
        {
            yield return new ValidationResult(
                $"Styling must be one of: {string.Join(", ", AllowedStyling.Order())}.",
                [nameof(Styling)]);
        }

        if (!ComponentNamePattern.IsMatch(ComponentName))
        {
            yield return new ValidationResult(
                "ComponentName must be PascalCase, start with an uppercase letter, and contain only letters or numbers.",
                [nameof(ComponentName)]);
        }
    }
}
