namespace SmartClinicManagement.Application.Common;


/// Real outcome of an attempted email send. Services return this instead of
/// swallowing SMTP exceptions silently, so controllers can report the true
/// delivery status to the client instead of assuming "email sent" just
/// because the surrounding DB operation successed

public class EmailSendResult
{
    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public static EmailSendResult Ok() => new() { Success = true };

    public static EmailSendResult Fail(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };
}
