using ConsoleApp1;
using System.Diagnostics.Metrics;

namespace LoggerClient
{
    public class LoggerClient : IDisposable
    {
        public static LoggerClient Instance { get; } = new ();

        private readonly Communication _communication;
        private LoggerClient()
        {
            _communication = new Communication(new Settings());
        }

        private int _sendCounter;
        public async void SendContent(string fileName, string content)
        {
            Interlocked.Increment(ref _sendCounter);
            await _communication.SendContent(fileName, content);
            Interlocked.Decrement(ref _sendCounter);
        }

        public bool AreAllMessagesSent => _sendCounter == 0;

        public int WaitingResponseCounter => _sendCounter;

        public void Dispose()
        {
            _communication?.Dispose();
        }
    }
}
