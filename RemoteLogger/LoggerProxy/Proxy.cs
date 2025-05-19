using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LoggerProxy
{
    public class Proxy : IDisposable
    {
        private static readonly Proxy Instance;

        private readonly bool _useRemoteLogging;
        private bool _disposedValue;
        private readonly string _processId;
        private readonly StreamWriter _writer;

        // Replace with the actual server address.
        private const string ServerIp = "172.22.144.60:5002";
        private readonly bool _keepOrigServerIp = true;
        private static bool _isStarted;

        static Proxy()
        {
            Instance = new Proxy(true);
        }

        public static void Log(string fileName, string content)
        {
            if (!_isStarted)
            {
                Thread.Sleep(5000);
                _isStarted = true;
            }

            Instance.Inst_Log(fileName, content);
        }

        private Proxy(bool remoteLogging)
        {
            _useRemoteLogging = remoteLogging;

            if (_useRemoteLogging)
            {
                string loggerClientPath = UnZipLoggerClient();
                if (string.IsNullOrEmpty(loggerClientPath))
                {
                    return;
                }

                _processId = Process.GetCurrentProcess().Id.ToString();
                string receiverProcessPath = loggerClientPath;

                // Start the receiver process and pass the string as an argument
                Process.Start(receiverProcessPath, _processId);

                // The started client listens on this NamedPipe channel.
                NamedPipeClientStream pipeClientStream = new NamedPipeClientStream(".", "LoggerPipe", PipeDirection.Out);
                pipeClientStream.Connect();
                _writer = new StreamWriter(pipeClientStream);
            }
        }

        private string UnZipLoggerClient()
        {
            var fullName = GetType().Assembly.Location;

            var directory = Path.GetDirectoryName(fullName);
            if (string.IsNullOrEmpty(directory))
            {
                return string.Empty;
            }

            var zipPath = Path.Combine(directory, "Resources", "LoggerClient.zip");
            if (File.Exists(zipPath))
            {
                var zipDirectoryPath = Path.GetDirectoryName(zipPath);
                if (string.IsNullOrEmpty(zipDirectoryPath))
                {
                    return string.Empty;
                }

                var extractPath = Path.Combine(zipDirectoryPath, "LoggerClient");

                if (!Directory.Exists(extractPath))
                {
                    Directory.CreateDirectory(extractPath);
                }

                var exePath = Path.Combine(extractPath, "LoggerClient.exe");
                if (!File.Exists(exePath))
                {
                    ZipFile.ExtractToDirectory(zipPath, extractPath);

                    if (!_keepOrigServerIp)
                    {
                        // update the address in client config to connect to the server.
                        var clientConfig = Path.Combine(extractPath, "LoggerClientConfig.json");
                        var configContent = File.ReadAllText(clientConfig);
                        configContent = configContent.Replace("localhost:5001", ServerIp);
                        File.WriteAllText(clientConfig, configContent);
                    }
                }

                return exePath;
            }

            return string.Empty;
        }

        protected void Inst_Log(string filename, string content)
        {
            if (_useRemoteLogging)
            {
                if (_writer != null)
                {
                    _writer.WriteLine(_processId);
                    _writer.WriteLine(filename);
                    _writer.WriteLine(content);
                    _writer.Flush();
                }
            }
            else
            {
                // TODO implement local logging
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _writer?.Close();
                    // _pipeClientStream?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Proxy()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public static void Dispose2()
        {
            Instance.Dispose();
        }

        static void ConnectWithTcpClient()
        {
            using (var client = new TcpClient("127.0.0.1", 12345))
            using (var stream = client.GetStream())
            {
                string fileContent = "File content to log dynamically.";
                byte[] data = Encoding.UTF8.GetBytes(fileContent);
                stream.Write(data, 0, data.Length); // Send message to logger
            }
        }
    }
}
