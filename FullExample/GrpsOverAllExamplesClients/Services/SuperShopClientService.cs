using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;
using GrpsOverAllExamplesClients.Enums;
using SuperShopServer;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Web;
using System.Xml.Linq;

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
        private LocalLogger _localLogger;

        public event PropertyChangedEventHandler? PropertyChanged;

        public SuperShopClientService()//ILoggerFactory loggerFactory)
        {
            // Logger = loggerFactory.CreateLogger<SuperShopClientService>();
            //  Logger.LogInformation($"{nameof(SuperShopClientService)} created");
            _localLogger = LocalLogger.Logger;
        }

        public bool ServiceStarted
        {
            get => _serviceStarted;
            set
            {
                _serviceStarted = value;
                string logMsg = _serviceStarted ? "Client started" : "Client stopped";
                _localLogger.Log(logMsg);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ServiceStarted)));
            }
        }

        public string Address { get; } = "https://localhost:5006";

        public bool Start()
        {
            if (ServiceStarted)
            {
                return true;
            }

            try
            {
                _channel = GrpcChannel.ForAddress(Address);
                // Logger.LogInformation($"{nameof(SuperShopClientService)} started with url address: {address}");
                ServiceStarted = true;
                _localLogger.Log($"Connected to server: {Address}");
            }
            catch (Exception ex)
            {
                // Logger.LogInformation(ex.ToString());
                _localLogger.Log($"Cannot onnect to {Address}: {ex}");
                return false;
            }

            return true;
        }

        public async Task<string> RegisterNewUser(string userName, string userPsw)
        {
            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["user"] = userName;
            queryParams["id"] = userPsw;

            // uses only HTTP to get the token
            using HttpClient client = new HttpClient();
            string address = $"{GetServerAddress()}/register?{queryParams}";
            return await client.GetStringAsync(address);
        }

        public async Task DisconnectAndReconnectWithAuthentication(string userName, string password)
        {
            Stop();

            try
            {
                //string token = await GetToken();
                string token = await GetTokenPost(userName, password);

                if (string.IsNullOrEmpty(token))
                {
                    _localLogger.Log("Login failed, user or passwork was not correct.");
                    return;
                }

                if (token.StartsWith("error"))
                {
                    _localLogger.Log($"Login failed: {token}");
                    return;
                }

                CallCredentials credentials = CallCredentials.FromInterceptor(interceptor: async (context, metadata) =>
                {
                    // The token could be asked here to get a fresh token for every call.
                    //string token = await GetTokenPost(userName, password);

                    // azt a felhaználót validáljuk aki a tokennel rendelkezik
                    // akkor hívódik meg amikor a call el van kérve a client-től,
                    // amikor a channel felépűl nem.
                    metadata.Add("Authorization", $"Bearer {token}");
                });

                // The previously requested token is used in the channel
                // Interceptor segítségével fogja a hívásokhoz hozzárakni a tokent.
                _channel = GrpcChannel.ForAddress(GetServerAddress(), new GrpcChannelOptions
                {
                    Credentials = ChannelCredentials.Create(new SslCredentials(), credentials),
                });
                ServiceStarted = true;
                _localLogger.Log($"Connected to server: {Address}, User: {userName}");
            }
            catch (Exception ex)
            {
                _localLogger.Log($"Cannot connect with user/psw to {Address}: {ex}");
            }
        }

        public void Stop()
        {
            ServiceStarted = false;
            _localLogger.Log("Client disconnected");
            Dispose();
        }

        //  public ILogger Logger { get; }


        public async Task SetMockValue(ServiceHealthTypes healthType, int value)
        {
            MockValueSetter.MockValueSetterClient client = GetMockValueSetterClient() ?? throw new Exception("Mock value setter client cannot be created.");
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

            Health.HealthClient client = GetHealthCheckClient() ?? throw new Exception("Healt check client cannot be created.");

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
            Health.HealthClient client = GetHealthCheckClient() ?? throw new Exception("Healt check client cannot be created.");

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

        private Health.HealthClient? GetHealthCheckClient()
        {
            if (_healthClient == null && _channel != null)
            {
                _healthClient = new Health.HealthClient(_channel);
            }
            return _healthClient;
        }

        private MockValueSetter.MockValueSetterClient? GetMockValueSetterClient()
        {
            if (_mockValueSetterclient == null && _channel != null)
            {
                _mockValueSetterclient = new MockValueSetter.MockValueSetterClient(_channel);
            }
            return _mockValueSetterclient;
        }

        protected virtual void Dispose(bool disposing)
        {
            //if (!disposedValue)
            //{
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _channel?.Dispose();
                    _channel = null;
                    _healthClient = null;
                    _mockValueSetterclient = null;
                    ServiceStarted = false;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            //}
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

        private string GetServerAddress()
        {
            return Address;
        }

        private async Task<string> GetTokenGet()
        {
            // uses only HTTP to get the token
            using HttpClient client = new HttpClient();
            string address = $"{GetServerAddress()}/token";
            return await client.GetStringAsync(address);
        }

        private async Task<string> GetTokenPost(string username, string password)
        {
            using HttpClient client = new HttpClient();

            // Create the request payload
            var credentials = new { Username = username, Password = password };
            var jsonPayload = JsonSerializer.Serialize(credentials);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Send the POST request
            HttpResponseMessage response = await client.PostAsync($"{GetServerAddress()}/token", content);

            // Ensure the response is successful
            response.EnsureSuccessStatusCode();

            // Read and return the token from the response body
            return await response.Content.ReadAsStringAsync();
        }
    }
}
