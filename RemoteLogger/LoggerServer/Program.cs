using LoggerServer.Bll;
using LoggerServer.Helpers;
using LoggerServer.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

//Host.CreateDefaultBuilder(args)
//           .ConfigureWebHostDefaults(webBuilder =>
//           {
//               webBuilder.UseStartup<Startup>();

//               // Set the application URL explicitly
//               webBuilder.UseUrls("http://localhost:5000", "https://localhost:5001");
//           });

Host.CreateDefaultBuilder(args)
           .ConfigureWebHostDefaults(webBuilder =>
           {
               webBuilder.UseStartup<Program>();

               // Read the URL(s) from configuration
               var configuration = new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json")
                   .Build();

               var urls = configuration.GetSection("Kestrel:Endpoints").GetChildren()
                   .Select(endpoint => endpoint.GetValue<string>("Url"))
                   .ToArray();

               webBuilder.UseUrls(urls);
           });


// Add services to the container.
ConfigureServices(builder.Services);

var app = builder.Build();

//app.UseStaticFiles(new StaticFileOptions
//{
//    RequestPath = "/Protos",
//    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Protos")),
//    ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string> { { ".proto", "text/plain" } })// key fájl, value=ebben a formában adja vissza.
//});

// Configure the HTTP request pipeline.
app.MapGrpcService<LoggingService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

void ConfigureServices(IServiceCollection services)
{
    RegisterGrpcServices(services);
    ConfigureDatabase(services);
}

void RegisterGrpcServices(IServiceCollection services)
{
    // The setting is enough to create only once
    services.AddSingleton<ISettings, Settings>();
    services.AddBusinessServices(builder.Configuration);
    services.AddGrpc();
}
void ConfigureDatabase(IServiceCollection services)
{
}