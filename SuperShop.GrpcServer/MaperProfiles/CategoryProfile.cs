namespace SuperShop.GrpcServer.MaperProfiles
{
    public class CategoryProfile : AutoMapper.Profile
    {
        public CategoryProfile()
        {
            // source - target, if something cannot be converted by name and type then use .ForMember
            CreateMap<Category, GetCategoryResponse>().ForMember(resp => resp.Picture,
                config=>config.MapFrom(model=>ByteString.CopyFrom(model.Picture)));
        }
    }
}
