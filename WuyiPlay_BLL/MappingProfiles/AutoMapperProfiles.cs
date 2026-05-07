using AutoMapper;
using WuyiPlay_DAL.DTOS;
using WuyiPlay_DAL.Models;

namespace WuyiPlay_BLL.MappingProfiles;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        // User
        CreateMap<User, UserDto>();

        // Category
        CreateMap<Category, CategoryBasicDto>();
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products));

        // ── Product ──────────────────────────────────────────────────────────
        CreateMap<Product, ProductBasicDto>()
            .ForMember(dest => dest.PCode,       opt => opt.MapFrom(src => src.PCode))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Describe))
            .ForMember(dest => dest.CId,         opt => opt.MapFrom(src => src.CId ?? 0))
            .ForMember(dest => dest.Category,    opt => opt.MapFrom(src => src.CIdNavigation))
            .ForMember(dest => dest.ProductImages, opt => opt.MapFrom(src => src.ProductImages));

        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.PCode,       opt => opt.MapFrom(src => src.PCode))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Describe))
            .ForMember(dest => dest.CId,         opt => opt.MapFrom(src => src.CId ?? 0))
            .ForMember(dest => dest.Category,    opt => opt.MapFrom(src => src.CIdNavigation))
            .ForMember(dest => dest.ProductImages, opt => opt.MapFrom(src => src.ProductImages));

        // ProductImage
        CreateMap<ProductImage, ProductImageDto>();

        // Cart
        CreateMap<Cart, CartBasicDto>();
        CreateMap<Cart, CartDto>()
            .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.PIdNavigation));

        // Order
        CreateMap<Order, OrderBasicDto>();
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.User,    opt => opt.MapFrom(src => src.UIdNavigation))
            .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.PIdNavigation));

        // BalanceAuditLog
        CreateMap<BalanceAuditLog, BalanceAuditLogBasicDto>();
    }
}
