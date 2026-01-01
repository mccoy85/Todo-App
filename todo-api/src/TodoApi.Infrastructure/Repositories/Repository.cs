using Microsoft.EntityFrameworkCore;
using TodoApi.Core.Interfaces;
using TodoApi.Infrastructure.Data;

namespace TodoApi.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly TodoDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(TodoDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);

        if (entity is ISoftDeletable softDeletable && softDeletable.IsDeleted)
        {
            return null;
        }

        return entity;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
        {
            return false;
        }

        if (entity is ISoftDeletable softDeletable)
        {
            if (softDeletable.IsDeleted)
            {
                return false;
            }

            softDeletable.IsDeleted = true;
            softDeletable.DeletedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            await SaveChangesAsync();
            return true;
        }

        _dbSet.Remove(entity);
        await SaveChangesAsync();
        return true;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
