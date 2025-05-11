using Grpc.Core;
using Grpc.Net.Client;
using LoggerClient;
using LoggerServer;

namespace ConsoleApp1
{
    internal class Communication : IDisposable
    {
        private GrpcChannel _channel;
        private Logging.LoggingClient _client;
        private bool disposedValue;
        private string _timeStamp;
        private readonly Settings _settings;

        public Communication(Settings settings)
        {
            // 127.0.0.1
            // TODO GN certificate is switched off. Implement it in the service (under construction).
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var httpClient = new HttpClient(handler);

            string url = settings.ServerAddress;
           // url = "https://172.22.144.60:5001";


            _channel = GrpcChannel.ForAddress(url, new GrpcChannelOptions { HttpClient = httpClient });

            //_channel = GrpcChannel.ForAddress(settings.ServerAddress);
            _client = new Logging.LoggingClient(_channel);
            _timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            _settings = settings;
        }

        public async void SendContent(string fileName, string longText)
        {
            var request = new SendContentRequest
            {
                Content = longText,
                ClientName = $"{_settings.AgentName}_{_timeStamp}",
                FileName = fileName
            };

            using AsyncUnaryCall<SendContentReply> call = _client.SendContentAsync(request);
            var response = await call.ResponseAsync;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _channel.Dispose();
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Communication()
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
    }
}
