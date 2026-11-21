using SmartClinicManagement.Domain.Entities;

namespace SmartClinicManagement.Application.Interfaces;

public interface IPasswordRepository
{
    Task<PasswordDetails?> GetCurrentPasswordAsync(int userId);

    Task CreateAsync(PasswordDetails passwordDetails);

    Task UpdateAsync(PasswordDetails passwordDetails);
}