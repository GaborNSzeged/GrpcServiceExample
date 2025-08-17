using Grpc.Core;
using Grpc.Net.Client;
using GrpcService1;
using System.Security.Cryptography.X509Certificates;

string url = "https://localhost:5553";

if (true)
{

    var handler = new HttpClientHandler();

    handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    // Optional: Accept self-signed certificate (for development only)
    //handler.ClientCertificates.Add(new X509Certificate2("c:\\certs\\localhost.pfx", "123456"));

    var httpClient = new HttpClient(handler);
    var grpcChannel = GrpcChannel.ForAddress(url, new GrpcChannelOptions { HttpClient = httpClient });
    var client = new Greeter.GreeterClient(grpcChannel);

    AsyncUnaryCall<HelloReply> grpcCall = client.SayHelloAsync(new HelloRequest { Name = "Gabor" });

    HelloReply response = await grpcCall.ResponseAsync;

    Console.WriteLine(response.Message);
}
else
{
    await ForProduction();
}

Console.ReadLine();
async Task ForProduction()
{
    var handler = new HttpClientHandler();

    // This is the default behavior: validate server certificate
    // You can customize it if needed:
    handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, certChain, sslPolicyErrors) =>
    {
        // Optional: log or inspect cert details
        return sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;
    };

    handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, chain, errors) =>
    {
        // Accept self-signed cert only for dev
        Console.WriteLine($"Subject: {cert.Subject}");
        Console.WriteLine($"Issuer: {cert.Issuer}");
        Console.WriteLine($"Errors: {errors}");
        // return errors == SslPolicyErrors.None;
        return true;
    };


    // Mutual TLS (Client Certificate Authentication)
    // If your server requires the client to present a certificate:
    handler.ClientCertificates.Add(new X509Certificate2("c:\\certs\\localhost.pfx", "123456"));

    var httpClient = new HttpClient(handler);

    // Use the actual domain name that matches the server certificate
    var channel = GrpcChannel.ForAddress(url, new GrpcChannelOptions
    {
        HttpClient = httpClient
    });

    // Now use the channel to create your client
    var client = new Greeter.GreeterClient(channel);
    var reply = await client.SayHelloAsync(new HelloRequest { Name = "Gabor" });
    Console.WriteLine(reply.Message);
}