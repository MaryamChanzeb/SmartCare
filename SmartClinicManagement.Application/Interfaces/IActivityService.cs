using SmartClinicManagement.Application.DTOs.Activities;

namespace SmartClinicManagement.Application.Interfaces;

public interface IActivityService
{
    Task<List<ActivityDto>> GetAllAsync();

    Task<ActivityDto?> GetByIdAsync(int id);

    Task<ActivityDto> CreateAsync(CreateActivityDto dto);

    Task<ActivityDto> UpdateAsync(int id, UpdateActivityDto dto);

    Task<bool> DeleteAsync(int id);
}