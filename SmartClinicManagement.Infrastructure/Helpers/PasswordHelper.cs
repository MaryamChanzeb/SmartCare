using System.Security.Cryptography;
using System.Text;

namespace SmartClinicManagement.Infrastructure.Helpers;

public static class PasswordHelper
{
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();

        var bytes = Encoding.UTF8.GetBytes(password);

        var hash = sha256.ComputeHash(bytes);

        return Convert.ToHexString(hash).ToLower();
    }

    // Generates a random temporary password for newly registered users/patients. The plain text value is only ever emailed to
    // the user and is never persisted; only its 
    // HashPassword is stored in the database.
    public static string GenerateTemporaryPassword(int length = 10)
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@#$%";

        const string all = upper + lower + digits + special;

        var passwordChars = new char[length];

        // Guarantee at least one character from each category.
        passwordChars[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
        passwordChars[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
        passwordChars[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
        passwordChars[3] = special[RandomNumberGenerator.GetInt32(special.Length)];

        for (var i = 4; i < length; i++)
        {
            passwordChars[i] = all[RandomNumberGenerator.GetInt32(all.Length)];
        }

        // Shuffle so the guaranteed characters aren't always in the same position.
        for (var i = passwordChars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (passwordChars[i], passwordChars[j]) = (passwordChars[j], passwordChars[i]);
        }

        return new string(passwordChars);
    }
}