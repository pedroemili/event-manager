using EventHub.Application.Events.Commands;
using FluentValidation;

namespace EventHub.Application.Events.Validators;

public sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    private static readonly string[] AllowedVisibilities = ["Public", "Unlisted", "Private"];

    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es obligatorio.")
            .MaximumLength(200);

        RuleFor(x => x.ShortDescription)
            .MaximumLength(500);

        RuleFor(x => x.StartDate)
            .GreaterThan(DateTime.UtcNow.AddMinutes(-1))
            .WithMessage("La fecha de inicio debe ser en el futuro.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("La fecha de fin debe ser posterior a la fecha de inicio.");

        RuleFor(x => x.MaxAttendees)
            .GreaterThan(0).When(x => x.MaxAttendees.HasValue)
            .WithMessage("MaxAttendees debe ser mayor a 0.");

        RuleFor(x => x.AgeRestriction)
            .GreaterThanOrEqualTo(0).When(x => x.AgeRestriction.HasValue)
            .LessThanOrEqualTo(120).When(x => x.AgeRestriction.HasValue);

        RuleFor(x => x.Visibility)
            .Must(v => AllowedVisibilities.Contains(v))
            .WithMessage($"Visibility debe ser uno de: {string.Join(", ", AllowedVisibilities)}.");
    }
}

public sealed class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    private static readonly string[] AllowedVisibilities = ["Public", "Unlisted", "Private"];

    public UpdateEventCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
        RuleFor(x => x.Visibility)
            .Must(v => AllowedVisibilities.Contains(v));
    }
}

public sealed class CancelEventCommandValidator : AbstractValidator<CancelEventCommand>
{
    public CancelEventCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Indica un motivo de cancelación.")
            .MaximumLength(500);
    }
}
