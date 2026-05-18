using ProductAPI.Application.DTOs.Items;

namespace ProductAPI.Application.DTOs.Products;

public class ProductDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public IEnumerable<ItemDto> Items { get; set; } = Enumerable.Empty<ItemDto>();
}