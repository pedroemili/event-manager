using EventHub.Application.Tickets.Commands;
using FluentValidation;

namespace EventHub.Application.Tickets.Validators;

public sealed class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(x => x.TicketTypeId).NotEmpty();
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1).WithMessage("La cantidad debe ser al menos 1.")
            .LessThanOrEqualTo(20).WithMessage("La cantidad máxima por reserva es 20.");
    }
}

public sealed class ValidateTicketCommandValidator : AbstractValidator<ValidateTicketCommand>
{
    public ValidateTicketCommandValidator()
    {
        RuleFor(x => x.QrData).NotEmpty();
        RuleFor(x => x.EventId).NotEmpty();
    }
}
