using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;
using GrpsOverAllExamplesClients.Enums;
using Microsoft.Extensions.Logging;
using SuperShopServer;
using System.ComponentModel;
using System.Threading.Channels;

namespace GrpsOverAllExamplesClients.Services
{
    internal class SuperShopClientService : ISuperShopClientService
    {
        private GrpcChannel? _channel;
        private CancellationTokenSource? _continuousHelathCheckCts;
        private AsyncServerStreamingCall<HealthCheckResponse>? _continuousHealthCheckWatchCall;
        private Health.HealthClient? _healthClient;
        private MockValueSetter.MockValueSetterClient? _mockValueSetterclient;
        private bool disposedValue;
        private bool _serviceStarted;

        public event PropertyChangedEventHandler? PropertyChanged;

        public SuperShopClientService()//ILoggerFactory loggerFactory)
        {
            // Logger = loggerFactory.CreateLogger<SuperShopClientService>();
            //  Logger.LogInformation($"{nameof(SuperShopClientService)} created");
        }

        public bool ServiceStarted
        {
            get => _serviceStarted;
            set
            {
                _serviceStarted = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ServiceStarted)));
            }
        }

        public bool Start()
        {
            if (_serviceStarted)
            {
                return true;
            }
            try
            {
                string address = "https://localhost:5006";
                _channel = GrpcChannel.ForAddress(address);
                // Logger.LogInformation($"{nameof(SuperShopClientService)} started with url address: {address}");
                ServiceStarted = true;
            }
            catch (Exception ex)
            {
                // Logger.LogInformation(ex.ToString());
                return false;
            }

            return true;
        }

        public void Stop()
        {
            ServiceStarted = false;
            Dispose();
        }

        //  public ILogger Logger { get; }


        public async void SetMockValue(ServiceHealthTypes healthType, int value)
        {
            MockValueSetter.MockValueSetterClient client = GetMockValueSetterClient();
            SuperShopServer.ValueType valueType = SuperShopServer.ValueType.Cpu;

            if (healthType == ServiceHealthTypes.Disk)
            {
                valueType = SuperShopServer.ValueType.Disk;
            }
            else if (healthType == ServiceHealthTypes.Memory)
            {
                valueType = SuperShopServer.ValueType.Memory;
            }

            var request = new SetMockValueReques { ValueType = valueType, Value = value };
            var checkCall = client.SetMockValueAsync(request);
            await checkCall.ResponseAsync;
        }

        public async Task<string> GetInstantHealtCheck(ServiceHealthTypes serviceHealthType)
        {
            // Logger.LogInformation($"{nameof(SuperShopClientService)}.GetInstantHealtCheck()");

            Health.HealthClient client = GetHealthCheckClient();

            // This solution is good if you wan to get the health check only once.
            HealthCheckResponse resp;
            try
            {
                var checkCall = client.CheckAsync(new HealthCheckRequest() { Service = serviceHealthType.ToString() });
                resp = await checkCall.ResponseAsync;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return resp.Status.ToString();
        }

        public void StartContinousHelathCheck(Action<string> logger)
        {
            Health.HealthClient client = GetHealthCheckClient();
            _continuousHelathCheckCts = new CancellationTokenSource();
            _continuousHealthCheckWatchCall = client.Watch(new HealthCheckRequest(), cancellationToken: _continuousHelathCheckCts.Token);
            var watchTask = Task.Run(async () =>
            {
                try
                {
                    await foreach (var message in _continuousHealthCheckWatchCall.ResponseStream.ReadAllAsync())
                    {
                        // ide csak akkor jövünk, ha változás történt a server oldalon
                        string status = "Health of the server: " + message.Status;
                        //Console.WriteLine(status);
                        logger.Invoke(status);
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {
                    // caused by CancellationTokenSource.Dispose()
                    // cancelled exception is swollowed as it was thrown deliberatelly
                }
            });
        }

        public void StopContinousHelathCheck()
        {
            _continuousHelathCheckCts?.Dispose();
            _continuousHealthCheckWatchCall?.Dispose();
        }

        private Health.HealthClient GetHealthCheckClient()
        {
            if (_healthClient == null)
            {
                _healthClient = new Health.HealthClient(_channel);
            }
            return _healthClient;
        }

        private MockValueSetter.MockValueSetterClient GetMockValueSetterClient()
        {
            if (_mockValueSetterclient == null)
            {
                _mockValueSetterclient = new MockValueSetter.MockValueSetterClient(_channel);
            }
            return _mockValueSetterclient;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _channel?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SuperShopClientService()
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
