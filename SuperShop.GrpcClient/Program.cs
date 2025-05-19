// See https://aka.ms/new-console-template for more information

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using SuperShop.GrpcServer.Protos;
using System.Text.Json;
using System.Text;

Console.WriteLine("Hello, World!");
//ServiceCollection coll = new ServiceCollection();
//ServiceProvider serviceProvider = coll.BuildServiceProvider();
//serviceProvider.GetRequiredService<IMapper>();
//serviceProvider.Dispose();

static string GetServerAddress()
{
    return "https://localhost:7075";
}

string token = await GetToken();
CallCredentials credentials = CallCredentials.FromInterceptor(async (context, metadata) =>
{
    // azt a felhaználót validáljuk aki a tokennel rendelkezik
    // akkor hívódik meg amikor a call el van kérve a client-től
    metadata.Add("Authorization", $"Bearer {token}");
});

// The previously requested token is used in the channel
// Interceptor segítségével fogja a hívásokhoz hozzárakni a tokent.
using GrpcChannel grpcChannel = GrpcChannel.ForAddress(GetServerAddress(), new GrpcChannelOptions
{
    Credentials = ChannelCredentials.Create(new SslCredentials(), credentials),
});

//using GrpcChannel grpcChannel = GrpcChannel.ForAddress(GetServerAddress());
// one channel can be used with multiple services
// channel authorizálunk
CategoryService.CategoryServiceClient categoryServiceClient = new(grpcChannel);
ProductService.ProductServiceClient productServiceClient = new(grpcChannel);

// unary get category
if (false)
{
    Console.WriteLine("Ask for category 1.");
    using AsyncUnaryCall<GetCategoryResponse> call = categoryServiceClient.GetCategoryAsync(new GetCategoryRequest { CategoryId = 1 }, deadline: DateTime.UtcNow.AddSeconds(5));

    GetCategoryResponse response = await call.ResponseAsync;
    Metadata metadata = await call.ResponseHeadersAsync;
    Console.Write($"Category name: {response.CategoryName}");
}

// stream from server GetCategories
if (false)
{
    using AsyncServerStreamingCall<GetCategoryResponse> call = categoryServiceClient.GetCategories(new Empty());

    await foreach (GetCategoryResponse response in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"Category name: {response.CategoryName}");
    }
}

// client streaming (delete)
if (false)
{
    using AsyncClientStreamingCall<DeleteProductRequest, Empty> call = productServiceClient.DeleteProducts();

    while (true)
    {
        string s = Console.ReadLine();
        if (s == "exit")
        {
            break;
        }

        await call.RequestStream.WriteAsync(new DeleteProductRequest { ProductId = int.Parse(s) });
    }

    await call.RequestStream.CompleteAsync();
    await call.ResponseAsync;
}

// duplex streaming (create product)
if (false)
{
    using AsyncDuplexStreamingCall<CreateProductRequest, CreateProductResponse> call = productServiceClient.CreateProducts();

    Task writeTask = Task.Run(async () =>
    {
        while (true)
        {
            string s = Console.ReadLine();
            if (s == "exit")
            {
                break;
            }

            CreateProductRequest createProductRequest = new CreateProductRequest
            {
                ProductName = s,
                UnitPrice = 20,
                UnitsInStock = 1,
                CategoryId = 2
            };

            await call.RequestStream.WriteAsync(createProductRequest);
        }
        await call.RequestStream.CompleteAsync();
    });

    Task readTask = Task.Run(async () =>
    {
        List<Task> tasks = new List<Task>();
        await foreach (CreateProductResponse response in call.ResponseStream.ReadAllAsync())
        {
            tasks.Add(Task.Run(() => Console.WriteLine($"{response.ProductName} ({response.ProductId})")));
        }

        await Task.WhenAll(tasks);
    });

    await Task.WhenAll(writeTask, readTask);
}

// unary (get & update)
if (false)
{
    GetProductResponse response;
    int productId = 20;

    using (AsyncUnaryCall<GetProductResponse> getCall = productServiceClient.GetProductAsync(new GetProductRequest { ProductId = productId }))
    {
        response = await getCall.ResponseAsync;
    }

    using AsyncUnaryCall<EditProductResponse> editCall =
        productServiceClient.EditProductAsync(new EditProductRequest
        {
            ProductId = productId,
            ProductName = "Updated product name",
            CategoryId = response.CategoryId,
            UnitPrice = response.UnitPrice,
            UnitsInStock = 300, // to test validation in server side
            Discontinued = true, //response.Discontinued
        });

    var headers = await editCall.ResponseHeadersAsync;
    Console.WriteLine("Response: " + await editCall.ResponseAsync);
}

// server streaming products
if (false)
{
    // This call is authorized for age18, if under the drinks will be filtered
    using AsyncServerStreamingCall<GetProductResponse> call = productServiceClient.GetProducts(new Empty());

    await foreach (GetProductResponse response in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine(response);
    }
}

// server streaming (drinks)
if (false)
{
    using AsyncServerStreamingCall<GetDrinksResponse> call = productServiceClient.GetDrinks(new Empty());

    await foreach (GetDrinksResponse response in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine(response);
    }
}

// server streaming (by category)
if (false)
{
    using AsyncServerStreamingCall<GetProductsByCategoryResponse> call =
        productServiceClient.GetProductsByCategory(new GetProductsByCategoryRequest { CategoryId = 2 });

    await foreach (GetProductsByCategoryResponse response in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine(response);
    }
}

// test cache
if (true)
{
    int productId = 20;
    using (AsyncUnaryCall<GetProductResponse> getCall =
        productServiceClient.GetProductAsync(new GetProductRequest { ProductId = productId }))
    {
        var response = await getCall.ResponseAsync;
    }

    using (AsyncUnaryCall<GetProductResponse> getCall =
        productServiceClient.GetProductAsync(new GetProductRequest { ProductId = productId }))
    {
        var response = await getCall.ResponseAsync;
    }
}

Console.ReadLine();


static async Task<string> GetToken()
{
    // uses only HTTP to get the token
    using HttpClient client = new HttpClient();
    string address = $"{GetServerAddress()}/token";
    return await client.GetStringAsync(address);
}
static async Task<string> GetToken2(string username, string password)
{
    using HttpClient client = new HttpClient();

    // Create the request payload
    var credentials = new { Username = username, Password = password };
    var jsonPayload = JsonSerializer.Serialize(credentials);
    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

    // Send the POST request
    HttpResponseMessage response = await client.PostAsync("https://localhost:7075/token", content);

    // Ensure the response is successful
    response.EnsureSuccessStatusCode();

    // Read and return the token from the response body
    return await response.Content.ReadAsStringAsync();
}