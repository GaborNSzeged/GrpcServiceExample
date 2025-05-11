using System.IO.Pipes;

namespace LoggerClient
{
    internal class PipeServer
    {
        private string _masterProessId = string.Empty;

        internal void Start(string masterProcessId)
        {
            Console.WriteLine($"Started by process id value: {masterProcessId}");
            _masterProessId = masterProcessId;

            using var client = LoggerClient.Instance;

            using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("LoggerPipe", PipeDirection.In))
            {
                while (true)
                {
                    try
                    {
                        Console.WriteLine("Waiting for messages...");
                        pipeServer.WaitForConnection();

                        string[] receivedValues = new string[3];
                        using (var reader = new StreamReader(pipeServer))
                        {
                            int paramCounter = 0;
                            string? message;
                            while ((message = reader.ReadLine()) != null)
                            {
                                receivedValues[paramCounter++] = message;
                                if (paramCounter != 3)
                                {
                                    Console.WriteLine($"Received: {message}");
                                }

                                if (paramCounter == 3)
                                {
                                    string callerProcessId = receivedValues[0];

                                    if (callerProcessId != _masterProessId)
                                    {
                                        break;
                                    }

                                    string fileName = receivedValues[1];
                                    string content = receivedValues[2];

                                    client.SendContent(fileName, content);
                                    paramCounter = 0;
                                }
                            }
                            break;
                        }
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
