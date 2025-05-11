using GrpcService1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddTransient<IMyInterface, MyClass>();

var app = builder.Build();


//app.MapGrpcReflectionService();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();


public interface IMyInterface
{

}

public class MyClass : IMyInterface
{

}