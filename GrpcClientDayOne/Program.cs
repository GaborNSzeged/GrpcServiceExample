Console.WriteLine("Hello, World!");

// long lived object, reuse it as long as you can.
//using GrpcChannel grpcChannel = GrpcChannel.ForAddress("https://localhost:7221", new GrpcChannelOptions(){});
using GrpcChannel grpcChannel = GrpcChannel.ForAddress("https://localhost:7221");

GreeterService.GreeterServiceClient client = new GreeterService.GreeterServiceClient(grpcChannel);

// This is not good because the dispose will not work
// GreaterUserResponse r = await client.GreeterUserAsync(new GreeterUserRequest() { UserName = "Gabor" });
if (false)
{
    // simplest unary communication
    using AsyncUnaryCall<GreaterUserResponse> call = client.GreeterUserAsync(new GreeterUserRequest() { UserName = "Gabor" });
    GreaterUserResponse actualResponse = await call.ResponseAsync;

    Console.WriteLine(actualResponse.GreetingMessage);
}

// Unary but request is an array like (repeated)
if (false)
{
    GreetMultipleUsersRequest request = new();
    while (true)
    {
        string s = Console.ReadLine();
        if (s == "done")
        {
            break;
        }
        request.UserNames.Add(s);
    }

    using AsyncUnaryCall<Empty> call2 = client.GreetMultiplseUsersAsync(request);
    Empty empty = await call2.ResponseAsync;
}

// client-stream
if (false)
{
    using AsyncClientStreamingCall<GreeterUserRequest, Empty> call = client.GreetMultiplseUsersStream();
    IClientStreamWriter<GreeterUserRequest> requestStream = call.RequestStream;
    while (true)
    {
        string s = Console.ReadLine();
        if (s == "done")
        {
            break;
        }


        await requestStream.WriteAsync(new GreeterUserRequest() { UserName = s });
    }
    // close the request stream
    await requestStream.CompleteAsync();
    Empty result = await call.ResponseAsync;
}

// Stream from the server
if (false)
{
    GreetMultipleUsersRequest r = new ();
    while (true)
    {
        string s = Console.ReadLine();
        if (s == "done")
        {
            break;
        }
        r.UserNames.Add(s);
    }

    using AsyncServerStreamingCall<GreaterUserResponse> call4 = client.GreetMultiplseUsersStreamServer(r);
    var stream = call4.ResponseStream;
    await foreach (var req in stream.ReadAllAsync())
    {
        Console.WriteLine("Got response from server " + req.GreetingMessage);
    }
}

// Bidirectional - streaming (not async)
if (false)
{
    using AsyncDuplexStreamingCall<GreeterUserRequest, GreaterUserResponse> call = client.GreetMultiplseUsersUberStream();
    while (true)
    {
        string s = Console.ReadLine();
        if (s == "done")
        {
            break;
        }

        await call.RequestStream.WriteAsync(new GreeterUserRequest() { UserName = s });

        // the move next is awaken by the server
        if (await call.ResponseStream.MoveNext())
        {
            ProcessResponse(call.ResponseStream.Current);
        }

    }

    await call.RequestStream.CompleteAsync();
}

// Bidirectional - streaming (async) duplex==bidirectional
if (true)
{
    using AsyncDuplexStreamingCall<GreeterUserRequest, GreaterUserResponse> call5 = client.GreetMultiplseUsersUberStream();
    Task writeTask = Task.Run(async () =>
    {
        while (true)
        {
            string s = Console.ReadLine();
            if (s == "done")
            {
                break;
            }

            // critical sect
            await call5.RequestStream.WriteAsync(new GreeterUserRequest() { UserName = s });
            // critical sect
        }

        // be careful of this call
        await call5.RequestStream.CompleteAsync();
    });

    Task readTask = Task.Run(async () =>
    {
        await foreach (var response in call5.ResponseStream.ReadAllAsync())
        {
            Console.WriteLine("Response received: " + response.GreetingMessage);
            _ = Task.Run(() => ProcessResponse(response)).ContinueWith(r => Console.WriteLine(r.Exception), TaskContinuationOptions.OnlyOnFaulted);
        }
    });

    await Task.WhenAll(readTask, writeTask);

}

Console.WriteLine();

static void ProcessResponse(GreaterUserResponse current)
{
    Thread.Sleep(3000);
    Console.WriteLine("Response received and processed: " + current.GreetingMessage);
}

// grpc.Channeel.Dispose() because it leaves the scope it was created
