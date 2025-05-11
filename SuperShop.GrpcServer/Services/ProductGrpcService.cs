using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;

namespace SuperShop.GrpcServer.Services;

// It checks only if the authorization was successful
[Authorize]
public class ProductGrpcService : ProductService.ProductServiceBase
{
    private readonly IProductService _productService;
    private readonly IMapper _mapper;

    public ProductGrpcService(IProductService productService, IMapper mapper)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public override async Task<Empty> DeleteProducts(IAsyncStreamReader<DeleteProductRequest> requestStream, ServerCallContext context)
    {
        // List<Task> deleteTask = new List<Task>();
        await foreach (DeleteProductRequest request in requestStream.ReadAllAsync(context.CancellationToken))
        {
            // this can ruin the parallelism, but this depends on how the logic is implemented, E.g.: the delete may cause an error
            await _productService.DeleteProductAsync(request.ProductId, context.CancellationToken);
            //deleteTask.Add(_productService.DeleteProductAsync(request.ProductId));
        }

        // await Task.WhenAll(deleteTask);

        return new Empty();
    }

    public override async Task CreateProducts(IAsyncStreamReader<CreateProductRequest> requestStream, IServerStreamWriter<CreateProductResponse> responseStream, ServerCallContext context)
    {
        // List<Task> tasks = new();
        await foreach (CreateProductRequest request in requestStream.ReadAllAsync(context.CancellationToken))
        {
            Product product = await _productService.CreateProductAsync(_mapper.Map<Product>(request), context.CancellationToken);
            await responseStream.WriteAsync(_mapper.Map<CreateProductResponse>(product), context.CancellationToken);

            //Task<Product> createdTask = _productService.CreateProductAsync(product, context.CancellationToken);
            //tasks.Add(createdTask.ContinueWith(async r => await responseStream.WriteAsync(mapper.Map<CreateProductResponse>(r))));
        }

        // await Task.WhenAll(tasks);
    }

    public override async Task<GetProductResponse> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        return _mapper.Map<GetProductResponse>(await _productService.GetProductAsync(request.ProductId));
    }

    public override async Task<EditProductResponse> EditProduct(EditProductRequest request, ServerCallContext context)
    {
        return _mapper.Map<EditProductResponse>(
            await _productService.EditProductAsync(
                _mapper.Map<Product>(request), context.CancellationToken
            )
         );
    }

    // token is in the context, so it would be possible to validate the age of the user here but that is not nice at all.
    [Authorize(Policy = "adultPolicy")]
    public override async Task GetDrinks(Empty request, IServerStreamWriter<GetDrinksResponse> responseStream, ServerCallContext context)
    {
        foreach (Product drink in await _productService.GetDrinksAsync())
        {
            await responseStream.WriteAsync(_mapper.Map<GetDrinksResponse>(drink));
        }
    }

    // on the Bll side there is some age filternig for dirinks using the Calim=dateíOfBirth
    [AllowAnonymous]
    public override async Task GetProducts(Empty request, IServerStreamWriter<GetProductResponse> responseStream, ServerCallContext context)
    {
        foreach (Product product in await _productService.GetProductsAsync())
        {
            await responseStream.WriteAsync(_mapper.Map<GetProductResponse>(product));
        }
    }

   // [Authorize(Roles = "admin")]
    public override async Task GetProductsByCategory(GetProductsByCategoryRequest request, IServerStreamWriter<GetProductsByCategoryResponse> responseStream, ServerCallContext context)
    {
        foreach (Product product in await _productService.GetProductsAsync(request.CategoryId))
        {
            await responseStream.WriteAsync(_mapper.Map<GetProductsByCategoryResponse>(product));
        }
    }
}

