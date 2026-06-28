namespace EventHub.Application.Common.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendVerificationEmailAsync(string to, string token, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string to, string token, CancellationToken cancellationToken = default);
    Task SendTicketConfirmationAsync(string to, string eventName, CancellationToken cancellationToken = default);
    Task SendEventCancellationAsync(string to, string eventName, string reason, CancellationToken cancellationToken = default);
}