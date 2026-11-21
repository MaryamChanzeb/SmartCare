namespace SmartClinicManagement.Application.Common;




public class AccountCreationResult
{
    public int UserId { get; set; }

    public EmailSendResult Email { get; set; } = EmailSendResult.Fail("Email was not attempted.");
}
