using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartClinicManagement.Application.Common;
using SmartClinicManagement.Application.Interfaces;
using SmartClinicManagement.Infrastructure.Configuration;
using System.Net;
using System.Net.Mail;

namespace SmartClinicManagement.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        bool isHtml = true)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            throw new ArgumentException("Recipient email address is required.", nameof(toEmail));

        // Previously a missing/blank SMTP config silently produced a
        // near-empty SmtpClient that either hung or threw a confusing
        // low-level error. Fail fast with a clear, actionable message.
        if (!_settings.IsConfigured)
        {
            throw new InvalidOperationException(
                "Email is not configured. Set EmailSettings:SmtpServer, SenderEmail, Username, " +
                "Password and Port (e.g. in appsettings.json, user-secrets, or via the " +
                "EmailSettings__SmtpServer / EmailSettings__Username / EmailSettings__Password / " +
                "EmailSettings__Port environment variables) before sending mail.");
        }

        using var smtp = new SmtpClient(_settings.SmtpServer)
        {
            Port = _settings.Port,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password),
            EnableSsl = _settings.EnableSSL,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            // Previously unset (infinite default), so a stalled connection
            // to Gmail could hang the request. Fail within a bounded time
            // instead so callers get a real error back.
            Timeout = _settings.TimeoutMilliseconds
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };

        message.To.Add(toEmail);

        try
        {
            await smtp.SendMailAsync(message);

            _logger.LogInformation(
                "Email sent to {ToEmail} with subject '{Subject}'.", toEmail, subject);
        }
        catch (SmtpFailedRecipientsException ex)
        {
            throw new InvalidOperationException(
                $"The mail server rejected recipient '{toEmail}'. {ex.Message}", ex);
        }
        catch (SmtpException ex)
        {
            // The two failures that account for almost every "Gmail won't
            // send" report: wrong port/SSL combo, or a normal account
            // password used instead of a Gmail App Password.
            var hint = _settings.Port == 465
                ? " Port 465 (implicit SSL) is not reliably supported by SmtpClient — use port 587 with EnableSSL=true (STARTTLS) instead."
                : " If using Gmail: Username/Password must be a Gmail address and a 16-character App Password " +
                  "(Google Account > Security > 2-Step Verification > App passwords), not the normal account " +
                  "password — Gmail rejects plain account passwords for SMTP.";

            throw new InvalidOperationException(
                $"Failed to send email via SMTP ({_settings.SmtpServer}:{_settings.Port}): {ex.Message}.{hint}", ex);
        }
    }

    public async Task<EmailSendResult> TrySendEmailAsync(
        string toEmail,
        string subject,
        string body,
        bool isHtml = true)
    {
        try
        {
            await SendEmailAsync(toEmail, subject, body, isHtml);
            return EmailSendResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send email to {ToEmail} with subject '{Subject}'.",
                toEmail,
                subject);

            return EmailSendResult.Fail(ex.Message);
        }
    }
}
