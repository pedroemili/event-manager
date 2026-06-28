using EventHub.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventHub.Infrastructure.Services;

public sealed class EmailSettings
{
    public string Provider { get; set; } = "Console";
    public string FromAddress { get; set; } = "no-reply@eventhub.local";
    public string FromName { get; set; } = "EventHub";
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public bool SmtpEnableSsl { get; set; } = true;
}

public sealed class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        LogEmail(to, subject, body);
        return Task.CompletedTask;
    }

    public Task SendVerificationEmailAsync(string to, string token, CancellationToken cancellationToken = default)
    {
        var subject = "Verify your EventHub email";
        var body = $"Welcome to EventHub! Your verification token is: {token}\nIt expires in 24 hours.";
        LogEmail(to, subject, body);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string to, string token, CancellationToken cancellationToken = default)
    {
        var subject = "Reset your EventHub password";
        var body = $"Use this token to reset your password: {token}\nIt expires in 1 hour.";
        LogEmail(to, subject, body);
        return Task.CompletedTask;
    }

    public Task SendTicketConfirmationAsync(string to, string eventName, CancellationToken cancellationToken = default)
    {
        var subject = $"Your ticket for {eventName}";
        var body = $"Thanks for purchasing tickets for {eventName}. Your tickets are now available in your account.";
        LogEmail(to, subject, body);
        return Task.CompletedTask;
    }

    public Task SendEventCancellationAsync(string to, string eventName, string reason, CancellationToken cancellationToken = default)
    {
        var subject = $"Event cancelled: {eventName}";
        var body = $"The event '{eventName}' was cancelled. Reason: {reason}. A refund will be issued shortly.";
        LogEmail(to, subject, body);
        return Task.CompletedTask;
    }

    private void LogEmail(string to, string subject, string body)
    {
        _logger.LogInformation(
            "[Email:{Provider}] To: {To} | Subject: {Subject} | From: {From}",
            _settings.Provider, to, subject, _settings.FromAddress);
        _logger.LogDebug("Email body: {Body}", body);
    }
}
