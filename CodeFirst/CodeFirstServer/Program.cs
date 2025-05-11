using CodeFirstServer.Services;
using ProtoBuf.Grpc.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCodeFirstGrpc();

var app = builder.Build();
app.MapGrpcService<GreeterService>();

app.Run();
