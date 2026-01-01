using Microsoft.EntityFrameworkCore;
using TodoApi.Core.Entities;

namespace TodoApi.Infrastructure.Data;

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}
