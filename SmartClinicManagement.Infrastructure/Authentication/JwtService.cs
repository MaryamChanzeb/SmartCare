using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartClinicManagement.Application.Common;
using SmartClinicManagement.Application.Interfaces;
using SmartClinicManagement.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartClinicManagement.Infrastructure.Authentication;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;

    public JwtService(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(
                System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub,
                user.UserID.ToString()),

            new Claim(
                System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email,
                user.Email),

            new Claim(
                ClaimTypes.Name,
                $"{user.FirstName} {user.LastName}"),

            new Claim(
                ClaimTypes.Role,
                user.Role?.RoleName ?? string.Empty),

            new Claim(
                System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Key));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}