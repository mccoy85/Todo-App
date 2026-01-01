using Microsoft.EntityFrameworkCore;
using TodoApi.Core.Interfaces;
using TodoApi.Infrastructure.Data;

namespace TodoApi.Infrastructure.Repositories;

// Generic EF repository for basic CRUD operations.
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
    {   // Update the entity
        _dbSet.Update(entity);
        await SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        // Find the entity by ID
        var entity = await GetByIdAsync(id);
        // If not found, return false
        if (entity == null)
        {
            return false;
        }
        // If entity supports soft delete, mark as deleted
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
        // Otherwise, perform hard delete
        _dbSet.Remove(entity);
        await SaveChangesAsync();
        return true;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
