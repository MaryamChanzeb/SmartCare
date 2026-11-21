using SmartClinicManagement.Application.Common;
using SmartClinicManagement.Application.DTOs.Users;

namespace SmartClinicManagement.Application.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();

    Task<UserDto?> GetByIdAsync(int id);

    // Returns the new UserID plus the real outcome of the welcome/
    // credentials email, so the controller can report actual delivery
    // status instead of assuming success just because the account was
    // created.
    Task<AccountCreationResult> CreateAsync(CreateUserDto dto);

    // Same as CreateAsync, but named for callers (e.g. combined patient
    // registration) that immediately need the new UserID to link a
    // dependent record without having to re-fetch and match on email.
    Task<AccountCreationResult> RegisterAccountAsync(CreateUserDto dto);

    Task UpdateAsync(int id, UpdateUserDto dto);

    Task DeleteAsync(int id);
}
