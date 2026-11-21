using Microsoft.EntityFrameworkCore;
using SmartClinicManagement.Application.Interfaces;
using SmartClinicManagement.Persistence.Context;
using System.Linq.Expressions;

namespace SmartClinicManagement.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T>
    where T : class
{
    private readonly ApplicationDbContext _context;

    private readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<List<T>> GetAllAsync(
        Expression<Func<T, bool>> predicate)
    {
        return await _dbSet
            .Where(predicate)
            .ToListAsync();
    }


    public async Task<List<T>> GetAllAsync(
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.ToListAsync();
    }

    public async Task<List<T>> GetAllAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet.Where(predicate);

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.ToListAsync();
    }

    public async Task<(List<T> Items, int TotalCount)> GetAllOrderedAsync(
        Expression<Func<T, bool>>? predicate,
        Expression<Func<T, object>> orderByDescending,
        int? pageNumber,
        int? pageSize,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        // Sort is applied — and, when paging is requested, Skip/Take is
        // applied too — before ToListAsync(), so both translate into the
        // SQL query itself (ORDER BY / LIMIT-OFFSET) instead of pulling
        // every row into memory first.
        query = query.OrderByDescending(orderByDescending);

        var totalCount = await query.CountAsync();

        if (pageNumber.HasValue && pageSize.HasValue && pageSize.Value > 0)
        {
            var skip = (Math.Max(1, pageNumber.Value) - 1) * pageSize.Value;
            query = query.Skip(skip).Take(pageSize.Value);
        }

        var items = await query.ToListAsync();

        return (items, totalCount);
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }
    public async Task<int> CountAsync(
    Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.CountAsync(predicate);
    }

    public async Task<decimal> SumAsync(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, decimal>> selector)
    {
        return await _dbSet
            .Where(predicate)
            .SumAsync(selector);
    }
    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(predicate);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }
    public Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);

        return Task.CompletedTask;
    }
}
