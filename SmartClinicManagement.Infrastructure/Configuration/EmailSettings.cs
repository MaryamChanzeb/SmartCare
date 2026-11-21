namespace SmartClinicManagement.Infrastructure.Configuration;

public class EmailSettings
{
    public string SenderEmail { get; set; } = string.Empty;

    public string SenderName { get; set; } = string.Empty;

    // Default to Gmail's SMTP host so a bare/partial config still points
    // somewhere sensible; override via appsettings/user-secrets/env vars.
    public string SmtpServer { get; set; } = "smtp.gmail.com";

    // 587 = STARTTLS, the combo SmtpClient supports reliably with Gmail.
    // (Port 465 / implicit SSL is not well supported by System.Net.Mail.)
    public int Port { get; set; } = 587;

    public string Username { get; set; } = string.Empty;

    // For Gmail this MUST be a 16-character App Password, not the normal
    // account password (requires 2-Step Verification enabled on the
    // Google account, then a password generated at
    // https://myaccount.google.com/apppasswords).
    public string Password { get; set; } = string.Empty;

    public bool EnableSSL { get; set; } = true;

    // URL shown in credential emails so new users know where to log in.
    public string LoginUrl { get; set; } = "http://localhost:4200/login";

    // SmtpClient send timeout in milliseconds. Previously unset, so a
    // stalled connection to Gmail could hang the request indefinitely.
    public int TimeoutMilliseconds { get; set; } = 30000;

    // True once the minimum fields needed to actually attempt a send are
    // present. EmailService checks this before touching SmtpClient so a
    // blank/missing config fails with one clear message instead of a
    // confusing low-level SMTP exception (or a hang).
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(SmtpServer) &&
        !string.IsNullOrWhiteSpace(SenderEmail) &&
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password) &&
        Port > 0;
}
