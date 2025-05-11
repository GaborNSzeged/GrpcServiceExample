using MiddleWareService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();

app.UseMiddleware<MyMiddleWare1>();
app.UseMiddleware<MyMiddleWare2>();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcReflectionService();

app.Run();

// no interface!!!
public class MyMiddleWare1
{
    private readonly RequestDelegate _next;

    public MyMiddleWare1(RequestDelegate request)
    {
        _next = request;
    }

    public async Task InvokeAsync(HttpContext content)
    {
        //try
        //{
        //    await _next.Invoke(content);
        //}
        //catch (Exception ex)
        //{
        //    // This could be an exception handling
        //}

        if (content.Request.ContentType == "text/plain")
        {
            await content.Response.WriteAsJsonAsync("Hello world from Middleware :)");
        }
        else
        {
            // itt kell megadni hogy mit csinál
            await _next.Invoke(content);
        }
    }
}

public class MyMiddleWare2
{
    private readonly RequestDelegate _next;
    public MyMiddleWare2(RequestDelegate request)
    {
        _next = request;
    }

    public async Task InvokeAsync(HttpContext content)
    {
        // itt kell megadni hogy mit csinál
        await _next.Invoke(content);
    }
}
