using AutoMapper;
using ProductAPI.Application.Common;
using ProductAPI.Application.DTOs.Items;
using ProductAPI.Application.DTOs.Products;
using ProductAPI.Application.Interfaces;
using ProductAPI.Domain.Entities;
using ProductAPI.Domain.Exceptions;

namespace ProductAPI.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductDto>> GetAllProductsAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var paged = await _unitOfWork.Products.GetPagedAsync(pageNumber, pageSize, cancellationToken);
        return new PagedResult<ProductDto>
        {
            Items = _mapper.Map<IEnumerable<ProductDto>>(paged.Items),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }

    public async Task<ProductDto> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateProductAsync(
        CreateProductDto dto, string createdBy, CancellationToken cancellationToken = default)
    {
        var product = _mapper.Map<Product>(dto);
        product.CreatedBy = createdBy;
        product.CreatedOn = DateTime.UtcNow;

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateProductAsync(
        int id, UpdateProductDto dto, string modifiedBy, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        product.ProductName = dto.ProductName;
        product.ModifiedBy = modifiedBy;
        product.ModifiedOn = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        await _unitOfWork.Products.DeleteAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<ItemDto>> GetItemsByProductIdAsync(
        int productId, CancellationToken cancellationToken = default)
    {
        // Verify product exists
        _ = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), productId);

        var items = await _unitOfWork.Products.GetItemsByProductIdAsync(productId, cancellationToken);
        return _mapper.Map<IEnumerable<ItemDto>>(items);
    }
}