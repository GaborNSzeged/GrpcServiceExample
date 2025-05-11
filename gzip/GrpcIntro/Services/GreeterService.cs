namespace GrpcIntro.Services;

public class GreeterService : GreeterServiceBase
{
    public async override Task<GreetUserResponse> GreetUser(GreetUserRequest request, ServerCallContext context)
    {
        Console.WriteLine("Hello " + request.UserName);
        return new GreetUserResponse { GreetingMessage = "Hello" + request.UserName };
    }

    public async override Task<Empty> GreetMultipleUsers(GreetMultipleUsersRequest request, ServerCallContext context)
    {
        foreach (string userName in request.UserNames)
        {
            Console.WriteLine("Hello " + userName);
        }
        return new Empty();
    }

    public async override Task<Empty> GreetMultipleUsersStream(IAsyncStreamReader<GreetUserRequest> requestStream, ServerCallContext context)
    {
        await foreach (GreetUserRequest req in requestStream.ReadAllAsync())
        {
            Console.WriteLine("Hello " + req.UserName);
        }


        //while (await requestStream.MoveNext())
        //{
        //    GreetUserRequest req = requestStream.Current;
        //    Console.WriteLine("Hello " + req.UserName);
        //}

        return new Empty();
    }

    public override async Task GreetMultipleUsersStreamServer(GreetMultipleUsersRequest request, IServerStreamWriter<GreetUserResponse> responseStream, ServerCallContext context)
    {
        foreach (string userName in request.UserNames)
        {
            await responseStream.WriteAsync(new GreetUserResponse { GreetingMessage = "Hello " + userName });
            await Task.Delay(10_000);
        }
    }

    public override async Task GreetMultipleUsersUberStream(IAsyncStreamReader<GreetUserRequest> requestStream, IServerStreamWriter<GreetUserResponse> responseStream, ServerCallContext context)
    {
        await foreach (GreetUserRequest request in requestStream.ReadAllAsync())
        {
            Console.WriteLine("Request "+request.UserName+" received");
            Task.Run(async () =>
            {
                ProcessRequest(request);
                //critical sect
                await responseStream.WriteAsync(new GreetUserResponse { GreetingMessage = "Hello " + request.UserName });
                //critical sect
            });
        }
    }

    private void ProcessRequest(GreetUserRequest request)
    {
        Thread.Sleep(3000);
        Console.WriteLine("Request "+request.UserName+" processed");
    }
}
