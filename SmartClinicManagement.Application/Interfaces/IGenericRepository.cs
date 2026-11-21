using System.Linq.Expressions;

namespace SmartClinicManagement.Application.Interfaces;

public interface IGenericRepository<T> where T : class
{
 
    Task<List<T>> GetAllAsync();
    Task<List<T>> GetAllAsync(
        Expression<Func<T, bool>> predicate);

    Task<List<T>> GetAllAsync(
        params Expression<Func<T, object>>[] includes);

    Task<List<T>> GetAllAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes);
    Task<(List<T> Items, int TotalCount)> GetAllOrderedAsync(
        Expression<Func<T, bool>>? predicate,
        Expression<Func<T, object>> orderByDescending,
        int? pageNumber,
        int? pageSize,
        params Expression<Func<T, object>>[] includes);

    Task<T?> GetByIdAsync(int id);

    
    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes);

    Task<int> CountAsync(Expression<Func<T, bool>> predicate);

    Task<decimal> SumAsync(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, decimal>> selector);
    Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate);

   
    Task AddAsync(T entity);

  
    Task UpdateAsync(T entity);

  
    Task DeleteAsync(T entity);
}