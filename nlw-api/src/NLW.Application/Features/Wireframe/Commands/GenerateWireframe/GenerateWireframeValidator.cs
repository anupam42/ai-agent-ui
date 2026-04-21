using FluentValidation;

namespace NLW.Application.Features.Wireframe.Commands.GenerateWireframe;

public sealed class GenerateWireframeValidator : AbstractValidator<GenerateWireframeCommand>
{
    public GenerateWireframeValidator()
    {
        RuleFor(x => x.Prompt)
            .NotEmpty().WithMessage("Prompt is required.")
            .MinimumLength(3).WithMessage("Prompt must be at least 3 characters.")
            .MaximumLength(2000).WithMessage("Prompt cannot exceed 2000 characters.");
    }
}
