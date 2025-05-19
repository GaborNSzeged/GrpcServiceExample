using System.IO.Pipes;

namespace LoggerClient
{
    internal class PipeServer
    {
        private string _masterProcessId = string.Empty;

        internal void Start(string masterProcessId)
        {
            Console.WriteLine($"Started by process id value: {masterProcessId}");
            _masterProcessId = masterProcessId;

            using var client = LoggerClient.Instance;

            using (var pipeServer = new NamedPipeServerStream("LoggerPipe", PipeDirection.In))
            {
                while (true)
                {
                    try
                    {
                        Console.WriteLine("Waiting for messages...");
                        pipeServer.WaitForConnection();

                        var receivedValues = new string[3];
                        var paramCounter = 0;
                        using var reader = new StreamReader(pipeServer);

                        while (reader.ReadLine() is { } message)
                        {
                            receivedValues[paramCounter++] = message;
                            if (paramCounter != 3)
                            {
                                Console.WriteLine($"Received: {message}");
                            }

                            if (paramCounter != 3)
                            {
                                continue;
                            }

                            string callerProcessId = receivedValues[0];

                            if (callerProcessId != _masterProcessId)
                            {
                                break;
                            }

                            string fileName = receivedValues[1];
                            string content = receivedValues[2];

                            client.SendContent(fileName, content);
                            paramCounter = 0;
                        }
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{ex.Message}");
                        break;
                    }
                }

                Console.WriteLine($"++++++++ LoggerPipe disposed ++++++++++++++");
            }

            // This is needed to let some time to the gRPC to fininsh the trasfer. TODO Find a better way for it.
            Thread.Sleep(3000);
        }
    }
}
