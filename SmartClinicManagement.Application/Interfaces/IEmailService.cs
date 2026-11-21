using SmartClinicManagement.Application.Common;

namespace SmartClinicManagement.Application.Interfaces;

public interface IEmailService
{

    /// Sends the email. Throws on failure (missing/invalid SMTP config,
    /// auth rejected, network error, etc.) — use this when the caller wants
    /// to handle the exception itself.

    Task SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        bool isHtml = true);

    
    /// Same as SendEmailAsync but never throws. Wraps the send in a
    /// try/catch and returns the real outcome, so calling services can
    /// let a DB transaction (registration, booking, etc.) succeed while
    /// still reporting the true email delivery status back to the caller
    /// instead of assuming success.

    Task<EmailSendResult> TrySendEmailAsync(
        string toEmail,
        string subject,
        string body,
        bool isHtml = true);
}
