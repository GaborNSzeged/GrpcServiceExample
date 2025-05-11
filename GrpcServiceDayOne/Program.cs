var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();
app.MapGrpcReflectionService();
app.MapGrpcService<GrpcServiceDayOne.Services.GreeterService>();
app.Run();
