using GrpcServiceDepency.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection(); // DI

//builder.Services.AddTransient<IService,Service>();
//builder.Services.AddTransient<IService2,Service2>();

//builder.Services.AddTransient<ICoreService,CoreService>();
//builder.Services.AddTransient<ICoreService,CoreService2>();

builder.Services.AddScoped<CoreService>();
builder.Services.AddTransient<CoreService2>();

builder.Services.AddTransient<IService>(sp => new Service(sp.GetRequiredService<CoreService>()));
builder.Services.AddTransient<IService2>(sp => new Service2(sp.GetRequiredService<CoreService2>()));

//builder.Services.AddSingleton<ISuperInterface1, SuperImplementation>();
//builder.Services.AddSingleton<ISuperInterface2, SuperImplementation>();
// torn lifecycle

builder.Services.AddSingleton<SuperImplementation>();
builder.Services.AddSingleton<ISuperInterface1>(ctx => ctx.GetRequiredService<SuperImplementation>());
builder.Services.AddSingleton<ISuperInterface2>(ctx => ctx.GetRequiredService<SuperImplementation>());

// builder.Services.AddScoped<ICoreService,CoreService>();
// captive dependency

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcReflectionService(); // DI
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
