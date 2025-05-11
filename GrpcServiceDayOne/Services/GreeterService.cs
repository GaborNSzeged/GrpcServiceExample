using Google.Protobuf.WellKnownTypes;

namespace GrpcServiceDayOne.Services;

public class GreeterService : GreeterServiceBase
{
    // unary 
    public override async Task<GreaterUserResponse> GreeterUser(GreeterUserRequest request, ServerCallContext context)
    {
        DateTime deadLine = context.Deadline;
        return new GreaterUserResponse { GreetingMessage = "Hello " + request.UserName };
    }

    // unary but the request is a arraylike (repeated) with empty response
    public override async Task<Empty> GreetMultiplseUsers(GreetMultipleUsersRequest request, ServerCallContext context)
    {
        foreach (string name in request.UserNames)
        {
            Console.WriteLine(name);
        }

        return new Empty();
    }

    // client-streaming a streaming, response is empty
    public override async Task<Empty> GreetMultiplseUsersStream(IAsyncStreamReader<GreeterUserRequest> requestStream, ServerCallContext context)
    {
        //while (await requestStream.MoveNext())
        //{
        //    var req = requestStream.Current;
        //    Console.WriteLine("Hello " + req.UserName);
        //}

        // .Net 8 solution
        await foreach (var req in requestStream.ReadAllAsync())
        {
            Console.WriteLine("Hello " + req.UserName);
            // Task.Run(() => Console.WriteLine("Hello " + req.UserName));
        }

        // Check keepAlive function

        return new Empty();
    }

    // server-streaming, request is an array like (repeated). The response stream is gotten via method parameter. This method is void
    public override async Task GreetMultiplseUsersStreamServer(GreetMultipleUsersRequest request, IServerStreamWriter<GreaterUserResponse> responseStream,
        ServerCallContext context)
    {
        foreach (string name in request.UserNames)
        {
            await responseStream.WriteAsync(new GreaterUserResponse() { GreetingMessage = "Hello " + name });
            await Task.Delay(5_000);
        }

        // The response stream is closed automatically when the program leaves this method.
    }

    // bidirectional-streaming, 
    public override async Task GreetMultiplseUsersUberStream(IAsyncStreamReader<GreeterUserRequest> requestStream, IServerStreamWriter<GreaterUserResponse> responseStream,
        ServerCallContext context)
    {
        await foreach (var req in requestStream.ReadAllAsync())
        {
            Console.WriteLine("Request " + req.UserName + " received");
            // this task is not "await" so the WriteAsync can cause a problem
            _ = Task.Run(async () =>
            {
                ProcessRequest(req);
                // Critical section
                await responseStream.WriteAsync(new GreaterUserResponse() { GreetingMessage = "Hello " + req.UserName });
                // Critical section
            });

        }
    }

    private void ProcessRequest(GreeterUserRequest req)
    {
        Thread.Sleep(3000);
        Console.WriteLine("Hello " + req.UserName + " processed");
    }
}

