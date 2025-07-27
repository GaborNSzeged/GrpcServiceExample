// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

LoggerClient.LoggerClient.Instance.SendContent("data1.csv", "kkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkk");
LoggerClient.LoggerClient.Instance.SendContent("data2.csv", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
LoggerClient.LoggerClient.Instance.SendContent("data3.csv", "wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww");

int waitingCounter = 0;
while (!LoggerClient.LoggerClient.Instance.AreAllMessagesSent)
{
    if (waitingCounter == 10)
    {
        break;
    }

    Console.WriteLine($"Waiting for the server response {LoggerClient.LoggerClient.Instance.WaitingResponseCounter}...");
    Thread.Sleep(1000);
    waitingCounter++;
}

Console.WriteLine($"No replies recieved: {LoggerClient.LoggerClient.Instance.WaitingResponseCounter}");


