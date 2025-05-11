// See https://aka.ms/new-console-template for more information

using System.IO.Pipes;
using System.Net.Sockets;
using System.Net;
using System.Text;
using LoggerClient;

internal class Program
{
    static void Main(string[] args)
    {
        // Check if arguments are provided
        if (args.Length > 0)
        {
            // loacal pc (faster then network)
            var pipeServer = new PipeServer();
            pipeServer.Start(args[0]);
        }
        else
        {
            Console.WriteLine("Hello, World!");

            using var loggerClient = LoggerClient.LoggerClient.Instance;

            loggerClient.SendContent("TestFile.txt", new string('k', 10000));
            loggerClient.SendContent("TestFile.txt", new string('p', 10000));

            //using var communication = new Communication(new LoggerClient.Settings());
            //communication.SendContent("TestFile.txt", new string('k', 10000));

            Console.WriteLine("File content sent.");

            Console.ReadLine();
            loggerClient.SendContent("Final.txt", "Before closing the client channel.");
            Console.WriteLine("Press Enter to close the Client.");
            Console.ReadLine();
        }

        Console.WriteLine($"++++++++ Exit main ++++++++++++++");
        //Console.ReadLine();
    }



    // for over network
    static void Main2(string[] args)
    {
        var listener = new TcpListener(IPAddress.Loopback, 12345);
        listener.Start();

        Console.WriteLine("Logger process is waiting for incoming messages...");
        while (true)
        {
            using (var client = listener.AcceptTcpClient())
            using (var stream = client.GetStream())
            {
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Logged Message: {receivedMessage}");
            }
        }
    }
}
