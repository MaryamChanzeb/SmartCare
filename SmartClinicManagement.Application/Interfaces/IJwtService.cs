using SmartClinicManagement.Domain.Entities;

namespace SmartClinicManagement.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}