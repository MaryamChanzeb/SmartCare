namespace SmartClinicManagement.Application.Common;


public class DuplicateEmailException : Exception
{
    public DuplicateEmailException(string message) : base(message)
    {
    }
}
