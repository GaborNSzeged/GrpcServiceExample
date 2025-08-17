
using GrpcInLib.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace GrpcInLib;

public class Server
{
    public Server()
    {
        //hostBuilder = Host.CreateDefaultBuilder()
        //            .ConfigureWebHostDefaults(webBuilder =>
        //            {
        //                //webBuilder.UseStartup<Server>();

        //                // Set the application URL explicitly
        //                webBuilder.UseUrls("http://localhost:5550", "https://localhost:5551");
        //            });

        //host = hostBuilder.Build();

    }

    WebApplication? app;

    public void Start()
    {
        Task.Run(() =>
        {
            string ip = "127.0.0.1";
            int port = 5553;

            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(System.Net.IPAddress.Parse(ip), port, listenOptions =>
                {
                    // ** Admin **
                    // 1.Create
                    // $cert = New-SelfSignedCertificate -CertStoreLocation Cert:\LocalMachine\My -DnsName "localhost"
                    // 2.
                    // New-Item -ItemType Directory -Path "C:\certs"
                    // 3. Export
                    // $password = ConvertTo-SecureString -String "123456" -Force -AsPlainText
                    // Export-PfxCertificate -Cert $cert -FilePath "C:\certs\localhost.pfx" -Password $password
                    /*
                     Want to Verify the Certificate?
                    You can open the Certificates MMC:
                    Press Win + R, type mmc, and hit Enter.
                    Go to File > Add/Remove Snap-in.
                    Add Certificates, choose Computer account, then Local computer.
                    Navigate to Certificates > Personal > Certificates to see your new cert.
                     */
                    if (true)
                    {
                        // listenOptions.UseHttps("c:\\certs\\localhost.pfx", "123456");

                        listenOptions.UseHttps(httpsOptions =>
                        {
                            httpsOptions.ServerCertificate = new X509Certificate2("c:\\certs\\localhost.pfx", "123456");
                            if (true)
                            {
                                // user this verion if you require that the client should have the crertificate.
                                httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                                httpsOptions.ClientCertificateValidation = (cert, chain, errors) =>
                                {
                                    // Validate client cert here
                                    // RemoteCertificateChainErrors
                                    //return errors == SslPolicyErrors.None;
                                    return true;
                                };
                            }
                        });
                    }
                    // defaut is http1http2
                    //listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
                });
            });

            // Add services to the container.
            ConfigureServices(builder.Services);

            app = builder.Build();

            app.MapGrpcService<GreeterService>();
            app.Run();
        });
    }

    public void Stop()
    {
        app?.StopAsync().Wait();
    }

    void ConfigureServices(IServiceCollection services)
    {
        RegisterGrpcServices(services);
        ConfigureDatabase(services);
    }

    void RegisterGrpcServices(IServiceCollection services)
    {
        // The setting is enough to create only once
        //services.AddSingleton<ISettings, Settings>();
        //services.AddBusinessServices(builder.Configuration);
        services.AddSingleton<MbdParameters>();
        services.AddGrpc();
    }
    void ConfigureDatabase(IServiceCollection services)
    {
    }
}
