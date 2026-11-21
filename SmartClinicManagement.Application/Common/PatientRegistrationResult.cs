namespace SmartClinicManagement.Application.Common;


/// Returned by IPatientService.RegisterPatientAsync: the new Patient/User
/// IDs plus the real outcome of the welcome/credentials email.

public class PatientRegistrationResult
{
    public int PatientId { get; set; }

    public int UserId { get; set; }

    public EmailSendResult Email { get; set; } = EmailSendResult.Fail("Email was not attempted.");
}
