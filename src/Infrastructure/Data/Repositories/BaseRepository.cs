using Microsoft.EntityFrameworkCore;
using ProductAPI.Application.Interfaces;
using ProductAPI.Infrastructure.Data;

namespace ProductAPI.Infrastructure.Data.Repositories;

public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<T> DbSet;

    protected BaseRepository(ApplicationDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await DbSet.FindAsync(new object?[] { id }, cancellationToken);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await DbSet.AddAsync(entity, cancellationToken);

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }
}