using Microsoft.EntityFrameworkCore.Storage;
using ProductAPI.Application.Interfaces;
using ProductAPI.Infrastructure.Data.Repositories;

namespace ProductAPI.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public IProductRepository Products { get; }

    public UnitOfWork(ApplicationDbContext context, IProductRepository products)
    {
        _context = context;
        Products = products;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        => _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
            await _transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
            await _transaction.RollbackAsync(cancellationToken);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}