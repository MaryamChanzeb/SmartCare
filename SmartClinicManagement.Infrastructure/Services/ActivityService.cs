using SmartClinicManagement.Application.DTOs.Activities;
using SmartClinicManagement.Application.Interfaces;
using SmartClinicManagement.Domain.Entities;

namespace SmartClinicManagement.Infrastructure.Services;

public class ActivityService : IActivityService
{
    private readonly IUnitOfWork _unitOfWork;

    public ActivityService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ActivityDto>> GetAllAsync()
    {
        var activities = await _unitOfWork
            .Repository<Activity>()
            .GetAllAsync(x => !x.IsDeleted);

        return activities.Select(x => new ActivityDto
        {
            ActivityID = x.ActivityID,
            ActivityName = x.ActivityName,
            Description = x.Description,
            MenuURL = x.MenuURL,
            Status = x.Status
        }).ToList();
    }

    public async Task<ActivityDto?> GetByIdAsync(int id)
    {
        var activity = await _unitOfWork
            .Repository<Activity>()
            .FirstOrDefaultAsync(x => x.ActivityID == id && !x.IsDeleted);

        if (activity == null)
            return null;

        return new ActivityDto
        {
            ActivityID = activity.ActivityID,
            ActivityName = activity.ActivityName,
            Description = activity.Description,
            MenuURL = activity.MenuURL,
            Status = activity.Status
        };
    }

    public async Task<ActivityDto> CreateAsync(CreateActivityDto dto)
    {
        bool exists = await _unitOfWork
            .Repository<Activity>()
            .AnyAsync(x =>
                x.ActivityName.ToLower() == dto.ActivityName.ToLower()
                && !x.IsDeleted);

        if (exists)
            throw new Exception("Activity already exists.");

        var activity = new Activity
        {
            ActivityName = dto.ActivityName,
            Description = dto.Description,
            MenuURL = dto.MenuURL,
            Status = dto.Status
        };

        await _unitOfWork
            .Repository<Activity>()
            .AddAsync(activity);

        await _unitOfWork.SaveChangesAsync();

        return new ActivityDto
        {
            ActivityID = activity.ActivityID,
            ActivityName = activity.ActivityName,
            Description = activity.Description,
            MenuURL = activity.MenuURL,
            Status = activity.Status
        };
    }


    public async Task<ActivityDto> UpdateAsync(int id, UpdateActivityDto dto)
    {
        var repository = _unitOfWork.Repository<Activity>();

        var activity = await repository
            .FirstOrDefaultAsync(x => x.ActivityID == id && !x.IsDeleted);

        if (activity == null)
            throw new Exception("Activity not found.");

        bool duplicate = await repository.AnyAsync(x =>
            x.ActivityName.ToLower() == dto.ActivityName.ToLower()
            && x.ActivityID != id
            && !x.IsDeleted);

        if (duplicate)
            throw new Exception("Activity name already exists.");

        activity.ActivityName = dto.ActivityName;
        activity.Description = dto.Description;
        activity.MenuURL = dto.MenuURL;
        activity.Status = dto.Status;

        await repository.UpdateAsync(activity);

        await _unitOfWork.SaveChangesAsync();

        return new ActivityDto
        {
            ActivityID = activity.ActivityID,
            ActivityName = activity.ActivityName,
            Description = activity.Description,
            MenuURL = activity.MenuURL,
            Status = activity.Status
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var repository = _unitOfWork.Repository<Activity>();

        var activity = await repository
            .FirstOrDefaultAsync(x => x.ActivityID == id && !x.IsDeleted);

        if (activity == null)
            throw new Exception("Activity not found.");

        activity.IsDeleted = true;

        await repository.UpdateAsync(activity);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}