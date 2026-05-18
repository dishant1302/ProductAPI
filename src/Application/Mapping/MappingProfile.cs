using AutoMapper;
using ProductAPI.Application.DTOs.Items;
using ProductAPI.Application.DTOs.Products;
using ProductAPI.Domain.Entities;

namespace ProductAPI.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product mappings
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore());

        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
            .ForMember(dest => dest.Items, opt => opt.Ignore());

        // Item mappings
        CreateMap<Item, ItemDto>();
        CreateMap<CreateItemDto, Item>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.Product, opt => opt.Ignore());
    }
}