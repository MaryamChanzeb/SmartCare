using SmartClinicManagement.Application.DTOs.Doctors;

public interface IDoctorService
{
    Task<List<DoctorDto>> GetAllAsync();

    Task<DoctorDto?> GetByIdAsync(int id);

    Task CreateAsync(CreateDoctorDto dto);
    Task RegisterDoctorAsync(RegisterDoctorDto dto);
    Task UpdateAsync(int id, UpdateDoctorDto dto);

    Task DeleteAsync(int id);
}