using ProductAPI.Application.DTOs.Items;

namespace ProductAPI.Application.DTOs.Products;

public class CreateProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public List<CreateItemDto> Items { get; set; } = new();
}