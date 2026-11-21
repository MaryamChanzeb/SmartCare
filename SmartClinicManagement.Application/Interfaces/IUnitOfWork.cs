using SmartClinicManagement.Application.Interfaces;

namespace SmartClinicManagement.Application.Interfaces;

public interface IUnitOfWork
{
    IGenericRepository<T> Repository<T>() where T : class;

    Task<int> SaveChangesAsync();
}