namespace SuperShop.GrpcServer.MaperProfiles;

public class ProductProfile : AutoMapper.Profile
{
    public ProductProfile()
    {
        CreateMap<Product, CreateProductResponse>();
        CreateMap<CreateProductRequest, Product>();
        CreateMap<Product, GetProductResponse>();
        CreateMap<EditProductRequest, Product>();
        CreateMap<Product, EditProductResponse>();
        CreateMap<Product, GetDrinksResponse>();
        CreateMap<Product, GetProductsByCategoryResponse>();
    }
}

