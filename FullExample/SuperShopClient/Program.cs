// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


//GrpcChannel channel = GrpcChannel.ForAddress("https://localhost:7246");
//Health.HealthClient client = new Health.HealthClient(channel);

// This solution is good if you wan to get the health check only once.
//var checkCall = client.CheckAsync(new HealthCheckRequest());
//var resp = await checkCall.ResponseAsync;
//Console.WriteLine(resp.Status);