namespace SmartClinicManagement.Infrastructure.Helpers;


public static class EmailTemplates
{
    private const string BrandColor = "#0d6efd";

    private static string Layout(string title, string bodyHtml) => $@"
<div style=""font-family: Segoe UI, Arial, sans-serif; background-color:#f4f6f8; padding:24px;"">
  <div style=""max-width:600px;margin:0 auto;background:#ffffff;border-radius:8px;overflow:hidden;border:1px solid #e5e7eb;"">
    <div style=""background:{BrandColor};padding:20px 24px;"">
      <h1 style=""color:#ffffff;margin:0;font-size:20px;"">Smart Clinic Management</h1>
    </div>
    <div style=""padding:24px;color:#1f2937;"">
      <h2 style=""margin-top:0;font-size:18px;color:#111827;"">{title}</h2>
      {bodyHtml}
    </div>
    <div style=""padding:16px 24px;background:#f9fafb;color:#6b7280;font-size:12px;border-top:1px solid #e5e7eb;"">
      This is an automated message from Smart Clinic Management. Please do not reply to this email.
    </div>
  </div>
</div>";

    private static string InfoRow(string label, string value) => $@"
        <tr>
          <td style=""padding:8px 12px;border:1px solid #e5e7eb;background:#f9fafb;color:#374151;""><b>{label}</b></td>
          <td style=""padding:8px 12px;border:1px solid #e5e7eb;color:#111827;"">{value}</td>
        </tr>";

    public static string AppointmentNotification(
        string patientName,
        string doctorName,
        string departmentName,
        DateOnly appointmentDate,
        TimeOnly appointmentTime,
        string confirmationStatus,
        string actionHeading,
        string introMessage)
    {
        var rows =
            InfoRow("Patient", patientName) +
            InfoRow("Doctor", doctorName) +
            InfoRow("Department", string.IsNullOrWhiteSpace(departmentName) ? "N/A" : departmentName) +
            InfoRow("Date", appointmentDate.ToString("dd-MMM-yyyy")) +
            InfoRow("Time", appointmentTime.ToString("hh:mm tt")) +
            InfoRow("Status", confirmationStatus);

        var body = $@"
            <p>Dear {patientName},</p>
            <p>{introMessage}</p>
            <table style=""border-collapse:collapse;width:100%;margin-top:12px;"">
                {rows}
            </table>
            <p style=""margin-top:20px;"">Thank you for choosing Smart Clinic.</p>";

        return Layout(actionHeading, body);
    }

    public static string PatientWelcome(
        string patientName,
        string username,
        string temporaryPassword,
        string roleName,
        string loginUrl)
    {
        var rows =
            InfoRow("Name", patientName) +
            InfoRow("Username / Email", username) +
            InfoRow("Temporary Password", temporaryPassword) +
            InfoRow("Role", roleName);

        var body = $@"
            <p>Dear {patientName},</p>
            <p>Welcome to Smart Clinic! Your patient account has been created successfully by our reception team.</p>
            <table style=""border-collapse:collapse;width:100%;margin-top:12px;"">
                {rows}
            </table>
            <p style=""margin-top:20px;"">
                <b>Login instructions:</b><br/>
                1. Go to the Smart Clinic portal: <a href=""{loginUrl}"">{loginUrl}</a><br/>
                2. Sign in using the username/email and temporary password above.<br/>
                3. For your security, please change your password after your first login.
            </p>
            <p>We look forward to taking care of your health.</p>";

        return Layout("Welcome to Smart Clinic", body);
    }

    public static string UserCredentials(
        string fullName,
        string username,
        string temporaryPassword,
        string roleName,
        string loginUrl)
    {
        var rows =
            InfoRow("Name", fullName) +
            InfoRow("Username / Email", username) +
            InfoRow("Temporary Password", temporaryPassword) +
            InfoRow("Assigned Role", roleName);

        var body = $@"
            <p>Dear {fullName},</p>
            <p>An account has been created for you on Smart Clinic Management by the administrator.</p>
            <table style=""border-collapse:collapse;width:100%;margin-top:12px;"">
                {rows}
            </table>
            <p style=""margin-top:20px;"">
                <b>Login instructions:</b><br/>
                1. Go to the Smart Clinic portal: <a href=""{loginUrl}"">{loginUrl}</a><br/>
                2. Sign in using the username/email and temporary password above.<br/>
                3. For your security, please change your password after your first login.<br/>
                4. You will only be able to access the dashboard and features assigned to your role ({roleName}).
            </p>";

        return Layout("Your Smart Clinic Account Credentials", body);
    }

    // Used by public self-registration (AuthService.RegisterAsync), where
    // the user picks their own password, so there's no temporary password
    // to include — just a confirmation that the account is ready.
    public static string SelfRegistrationWelcome(
        string fullName,
        string email,
        string roleName,
        string loginUrl)
    {
        var rows =
            InfoRow("Name", fullName) +
            InfoRow("Username / Email", email) +
            InfoRow("Role", roleName);

        var body = $@"
            <p>Dear {fullName},</p>
            <p>Thanks for creating your Smart Clinic account — your registration was successful.</p>
            <table style=""border-collapse:collapse;width:100%;margin-top:12px;"">
                {rows}
            </table>
            <p style=""margin-top:20px;"">
                You can log in any time using the email and password you chose:<br/>
                <a href=""{loginUrl}"">{loginUrl}</a>
            </p>
            <p>We look forward to taking care of your health.</p>";

        return Layout("Welcome to Smart Clinic", body);
    }
}
