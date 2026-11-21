using Microsoft.EntityFrameworkCore;
using SmartClinicManagement.Application.Interfaces;
using SmartClinicManagement.Domain.Entities;
using SmartClinicManagement.Persistence.Context;

namespace SmartClinicManagement.Infrastructure.Repositories;

public class PasswordRepository : IPasswordRepository
{
    private readonly ApplicationDbContext _context;

    public PasswordRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PasswordDetails?> GetCurrentPasswordAsync(int userId)
    {
        return await _context.PasswordDetails
            .FirstOrDefaultAsync(x =>
                x.UserID == userId &&
                x.IsCurrent &&
                !x.IsDeleted);
    }

    public async Task CreateAsync(PasswordDetails passwordDetails)
    {
        await _context.PasswordDetails.AddAsync(passwordDetails);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(PasswordDetails passwordDetails)
    {
        _context.PasswordDetails.Update(passwordDetails);
        await _context.SaveChangesAsync();
    }
}