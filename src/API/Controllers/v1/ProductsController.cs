using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductAPI.Application.Common;
using ProductAPI.Application.DTOs.Items;
using ProductAPI.Application.DTOs.Products;
using ProductAPI.Application.Interfaces;

namespace ProductAPI.API.Controllers.v1;

/// <summary>CRUD operations for Products and their related Items.</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        ICurrentUserService currentUser,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>Get a paginated list of all products.</summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10, max: 100)</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        _logger.LogInformation("Fetching products page {Page} size {Size}", pageNumber, pageSize);
        var result = await _productService.GetAllProductsAsync(pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<ProductDto>>.SuccessResult(result));
    }

    /// <summary>Get a specific product by its ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        var product = await _productService.GetProductByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ProductDto>.SuccessResult(product));
    }

    /// <summary>Create a new product. Items can be included optionally.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductDto dto,
        CancellationToken cancellationToken = default)
    {
        var createdBy = _currentUser.Email ?? "system";
        _logger.LogInformation("Creating product '{Name}' by {User}", dto.ProductName, createdBy);

        var product = await _productService.CreateProductAsync(dto, createdBy, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            ApiResponse<ProductDto>.SuccessResult(product, "Product created successfully."));
    }

    /// <summary>Update an existing product's name.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateProductDto dto,
        CancellationToken cancellationToken = default)
    {
        var modifiedBy = _currentUser.Email ?? "system";
        var product = await _productService.UpdateProductAsync(id, dto, modifiedBy, cancellationToken);
        return Ok(ApiResponse<ProductDto>.SuccessResult(product, "Product updated successfully."));
    }

    /// <summary>Delete a product and all its associated items.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting product {Id}", id);
        await _productService.DeleteProductAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Get all items belonging to a specific product.</summary>
    [HttpGet("{id:int}/items")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetItems(int id, CancellationToken cancellationToken = default)
    {
        var items = await _productService.GetItemsByProductIdAsync(id, cancellationToken);
        return Ok(ApiResponse<IEnumerable<ItemDto>>.SuccessResult(items));
    }
}