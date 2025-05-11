
using GrpcFileServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddSingleton<IFileRepository, FileRepository>();

var app = builder.Build();
app.MapGrpcService<FileService>();


app.Run();
