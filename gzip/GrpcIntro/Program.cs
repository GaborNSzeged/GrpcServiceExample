var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc(opts =>
{
    opts.ResponseCompressionAlgorithm = "gzip";
});
builder.Services.AddGrpcReflection();

var app = builder.Build();
app.MapGrpcReflectionService();
app.MapGrpcService<GrpcIntro.Services.GreeterService>();
app.Run();
