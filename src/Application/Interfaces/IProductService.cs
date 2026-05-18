using ProductAPI.Application.Common;
using ProductAPI.Application.DTOs.Items;
using ProductAPI.Application.DTOs.Products;

namespace ProductAPI.Application.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetAllProductsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<ProductDto> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateProductAsync(CreateProductDto dto, string createdBy, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto, string modifiedBy, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ItemDto>> GetItemsByProductIdAsync(int productId, CancellationToken cancellationToken = default);
}