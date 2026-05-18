using AutoMapper;
using FluentAssertions;
using Moq;
using ProductAPI.Application.Common;
using ProductAPI.Application.DTOs.Products;
using ProductAPI.Application.Interfaces;
using ProductAPI.Application.Mapping;
using ProductAPI.Application.Services;
using ProductAPI.Domain.Entities;
using ProductAPI.Domain.Exceptions;

namespace Application.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly IMapper _mapper;
    private readonly ProductService _sut; // System Under Test

    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepoMock = new Mock<IProductRepository>();
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _sut = new ProductService(_unitOfWorkMock.Object, _mapper);
    }

    // ── GetProductByIdAsync ────────────────────────────────────────────
    [Fact]
    public async Task GetProductByIdAsync_WhenProductExists_ShouldReturnMappedDto()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            ProductName = "Widget A",
            CreatedBy = "user@test.com",
            CreatedOn = DateTime.UtcNow
        };
        _productRepoMock
            .Setup(r => r.GetByIdAsync(1, default))
            .ReturnsAsync(product);

        // Act
        var result = await _sut.GetProductByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.ProductName.Should().Be("Widget A");
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _productRepoMock
            .Setup(r => r.GetByIdAsync(99, default))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _sut.GetProductByIdAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*99*");
    }

    // ── CreateProductAsync ─────────────────────────────────────────────
    [Fact]
    public async Task CreateProductAsync_WithValidDto_ShouldAddAndReturnProduct()
    {
        // Arrange
        var dto = new CreateProductDto { ProductName = "New Widget" };
        const string createdBy = "admin@test.com";

        _productRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), default))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateProductAsync(dto, createdBy);

        // Assert
        result.Should().NotBeNull();
        result.ProductName.Should().Be("New Widget");
        _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Product>(), default), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldSetCreatedByAndCreatedOn()
    {
        // Arrange
        var dto = new CreateProductDto { ProductName = "Test" };
        const string createdBy = "creator@test.com";
        Product? capturedProduct = null;

        _productRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), default))
            .Callback<Product, CancellationToken>((p, _) => capturedProduct = p)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _sut.CreateProductAsync(dto, createdBy);

        // Assert
        capturedProduct.Should().NotBeNull();
        capturedProduct!.CreatedBy.Should().Be(createdBy);
        capturedProduct.CreatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ── UpdateProductAsync ─────────────────────────────────────────────
    [Fact]
    public async Task UpdateProductAsync_WhenProductExists_ShouldReturnUpdatedDto()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            ProductName = "Old Name",
            CreatedBy = "user@test.com",
            CreatedOn = DateTime.UtcNow
        };
        var dto = new UpdateProductDto { ProductName = "New Name" };

        _productRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), default)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateProductAsync(1, dto, "editor@test.com");

        // Assert
        result.ProductName.Should().Be("New Name");
    }

    [Fact]
    public async Task UpdateProductAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _productRepoMock.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((Product?)null);

        // Act
        var act = () => _sut.UpdateProductAsync(99, new UpdateProductDto(), "user");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── DeleteProductAsync ─────────────────────────────────────────────
    [Fact]
    public async Task DeleteProductAsync_WhenProductExists_ShouldDeleteAndSave()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            ProductName = "To Delete",
            CreatedBy = "user",
            CreatedOn = DateTime.UtcNow
        };
        _productRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.DeleteAsync(It.IsAny<Product>(), default)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _sut.DeleteProductAsync(1);

        // Assert
        _productRepoMock.Verify(r => r.DeleteAsync(product, default), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _productRepoMock.Setup(r => r.GetByIdAsync(5, default)).ReturnsAsync((Product?)null);

        // Act
        var act = () => _sut.DeleteProductAsync(5);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── GetAllProductsAsync ────────────────────────────────────────────
    [Fact]
    public async Task GetAllProductsAsync_ShouldReturnPagedDtoResult()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, ProductName = "A", CreatedBy = "u", CreatedOn = DateTime.UtcNow },
            new() { Id = 2, ProductName = "B", CreatedBy = "u", CreatedOn = DateTime.UtcNow }
        };
        var paged = new PagedResult<Product>
        {
            Items = products,
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        _productRepoMock.Setup(r => r.GetPagedAsync(1, 10, default)).ReturnsAsync(paged);

        // Act
        var result = await _sut.GetAllProductsAsync(1, 10);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }
}