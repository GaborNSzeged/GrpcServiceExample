// See https://aka.ms/new-console-template for more information
using Grpc.Core;
using Grpc.Net.Client;
using GrpcService1;

Console.WriteLine("Hello, World!");

using GrpcChannel grpcChannel = GrpcChannel.ForAddress("https://localhost:7080");

var client = new Greeter.GreeterClient(grpcChannel);

AsyncUnaryCall<HelloReply> grpcCall = client.SayHelloAsync(new HelloRequest { Name = "Gabor" });

HelloReply response = await grpcCall.ResponseAsync;

Console.WriteLine(response.Message);

Console.ReadLine();
