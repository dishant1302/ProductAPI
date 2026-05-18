using Microsoft.EntityFrameworkCore;
using ProductAPI.Application.Common;
using ProductAPI.Application.Interfaces;
using ProductAPI.Domain.Entities;

namespace ProductAPI.Infrastructure.Data.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context) { }

    // Override to eagerly load Items
    public override async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await Context.Products
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<PagedResult<Product>> GetPagedAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = Context.Products
            .Include(p => p.Items)
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedOn);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<Item>> GetItemsByProductIdAsync(
        int productId, CancellationToken cancellationToken = default)
        => await Context.Items
            .Where(i => i.ProductId == productId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}