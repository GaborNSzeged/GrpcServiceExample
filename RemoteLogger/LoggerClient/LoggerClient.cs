using ConsoleApp1;

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

        public void SendContent(string fileName, string content)
        {
            _communication.SendContent(fileName, content);
        }

        public void Dispose()
        {
            _communication?.Dispose();
        }
    }
}
