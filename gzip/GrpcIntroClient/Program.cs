using GrpcChannel grpcChannel = GrpcChannel.ForAddress("https://localhost:7120");
GreeterServiceClient client = new(grpcChannel);

GreetUserRequest request = new GreetUserRequest() { UserName="Akos"};
GreetUserResponse resp = await client.GreetUserAsync(request, 
    new Metadata { {"grpc-internal-encoding-request","gzip" } });

//while (true)
//{
//    string s = Console.ReadLine();
//    if (s == "done")
//        break;

//    using AsyncUnaryCall<GreetUserResponse> call = client.GreetUserAsync(new GreetUserRequest { UserName = s });
//    GreetUserResponse response = await call.ResponseAsync;
//}

//GreetMultipleUsersRequest request = new();
//while (true)
//{
//    string s = Console.ReadLine();
//    if (s == "done")
//        break;
//    request.UserNames.Add(s);
//}

//using AsyncUnaryCall<Empty> call = client.GreetMultipleUsersAsync(request);
//Empty empty = await call.ResponseAsync;

//using AsyncClientStreamingCall<GreetUserRequest, Empty> call = client.GreetMultipleUsersStream();
//while (true)
//{
//    string s = Console.ReadLine();
//    if (s == "done")
//        break;
//    await call.RequestStream.WriteAsync(new GreetUserRequest { UserName = s });
//}
//await call.RequestStream.CompleteAsync();
//Empty result = await call.ResponseAsync;
//Console.ReadLine();

//GreetMultipleUsersRequest request = new GreetMultipleUsersRequest();
//while (true)
//{
//    string s = Console.ReadLine();
//    if (s == "done")
//        break;
//    request.UserNames.Add(s);
//}

//using AsyncServerStreamingCall<GreetUserResponse> call = client.GreetMultipleUsersStreamServer(request);
//await foreach (GreetUserResponse resp in call.ResponseStream.ReadAllAsync())
//{
//    Console.WriteLine(resp.GreetingMessage);
//}
//Console.ReadLine();

using AsyncDuplexStreamingCall<GreetUserRequest, GreetUserResponse> call = client.GreetMultipleUsersUberStream();
//while (true)
//{
//    string s = Console.ReadLine();
//    if (s == "done")
//        break;
//    await call.RequestStream.WriteAsync(new GreetUserRequest { UserName = s });

//    if (await call.ResponseStream.MoveNext()) //
//        ProcessResponse(call.ResponseStream.Current); //
//}
//await call.RequestStream.CompleteAsync();


Task writeTask = Task.Run(async () =>
{
    while (true)
    {
        string s = Console.ReadLine();
        if (s == "done")
            break;

        //critical sect
        await call.RequestStream.WriteAsync(new GreetUserRequest { UserName = s });
        //critical sect
    }
    await call.RequestStream.CompleteAsync();
});

Task readTask = Task.Run(async () =>
{
    await foreach (GreetUserResponse response in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine("Response received "+response.GreetingMessage);
        //ProcessResponse(response);
        Task.Run(() => ProcessResponse(response))
                .ContinueWith(r => Console.WriteLine(r.Exception),TaskContinuationOptions.OnlyOnFaulted);
    }
});

await Task.WhenAll(writeTask, readTask);
Console.ReadLine();

// call.Dispose();
// grpcChannel.Dispose();


static void ProcessResponse(GreetUserResponse current)
{
    Thread.Sleep(3000);
    Console.WriteLine("Response processed: " + current);
}