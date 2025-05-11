using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using System.Runtime.CompilerServices;

namespace SuperShop.GrpcServer.Services;

public class CategoryService : Protos.CategoryService.CategoryServiceBase
{
    private readonly ICategoryService _categoryService;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryService category, IMapper mapper)
    {
        _categoryService = category ?? throw new ArgumentNullException(nameof(category));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public override async Task<GetCategoryResponse> GetCategory(GetCategoryRequest request, ServerCallContext context)
    {
        await Task.Delay(10_000);

        Category c = await _categoryService.GetCategoryAsync(request.CategoryId, context.CancellationToken);
        return _mapper.Map<GetCategoryResponse>(c);

        //return new GetCategoryResponse()
        //{
        //    CategoryId = c.CategoryId,
        //    Description = c.Description,
        //    Picture = ByteString.CopyFrom(c.Picture),
        //    CategoryName = c.CategoryName
        //};
    }

    public override async Task GetCategories(Empty request, IServerStreamWriter<GetCategoryResponse> responseStream, ServerCallContext context)
    {
        // This is not really helpful as we do not got the data parallel but all at once (but this is just an example). 
        //  IReadOnlyList<Category> categories = await _categoryService.GetCategoriesAsync();

        // foreach (Category category in categories)
        await foreach (Category category in HackReaCategories(context.CancellationToken))
        {
            await responseStream.WriteAsync(_mapper.Map<GetCategoryResponse>(category));
            //await responseStream.WriteAsync(new GetCategoryResponse()
            //{
            //    CategoryId = category.CategoryId,
            //    Description = category.Description,
            //    Picture = ByteString.CopyFrom(category.Picture),
            //    CategorName = category.CategoryName
            //});
        }
    }

    private async IAsyncEnumerable<Category> HackReaCategories([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IReadOnlyList<Category> categories = await _categoryService.GetCategoriesAsync(cancellationToken);

        foreach (Category category in categories)
        {
            await Task.Delay(TimeSpan.FromSeconds(3), CancellationToken.None);
            yield return category;
        }
    }
}

