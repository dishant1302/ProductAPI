using ProductAPI.Application.Common;
using ProductAPI.Domain.Entities;

namespace ProductAPI.Application.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<PagedResult<Product>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<Item>> GetItemsByProductIdAsync(int productId, CancellationToken cancellationToken = default);
}