using Grpc.Core;
using GrpcServiceUnderTest;
using GrpcServiceUnderTest.Services;
using Moq;
using System.Threading.Channels;

namespace TestProject1
{
    public class TestAsyncStreamReader<T> : IAsyncStreamReader<T> where T : class
    {
        private readonly Channel<T> _channel;
        private readonly ServerCallContext _serverCallContext;

        public T Current { get; private set; }

        public TestAsyncStreamReader(ServerCallContext serverCallContext)
        {
            _channel = Channel.CreateUnbounded<T>();
            _serverCallContext = serverCallContext;
        }

        public void AddMessage(T message)
        {
            if (!_channel.Writer.TryWrite(message))
            {
                throw new InvalidOperationException("Unable to write message.");
            }
        }

        public void Complete()
        {
            _channel.Writer.Complete();
        }

        public async Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            _serverCallContext.CancellationToken.ThrowIfCancellationRequested();

            if (await _channel.Reader.WaitToReadAsync(cancellationToken) &&
                _channel.Reader.TryRead(out var message))
            {
                Current = message;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class TestServerCallContext : ServerCallContext
    {
        private readonly Metadata _requestHeaders;
        private readonly CancellationToken _cancellationToken;
        private readonly Metadata _responseTrailers;
        private readonly AuthContext _authContext;
        private readonly Dictionary<object, object> _userState;
        private WriteOptions? _writeOptions;

        public Metadata? ResponseHeaders { get; private set; }

        private TestServerCallContext(Metadata requestHeaders, CancellationToken cancellationToken)
        {
            _requestHeaders = requestHeaders;
            _cancellationToken = cancellationToken;
            _responseTrailers = new Metadata();
            _authContext = new AuthContext(string.Empty, new Dictionary<string, List<AuthProperty>>());
            _userState = new Dictionary<object, object>();
        }

        protected override string MethodCore => "MethodName";
        protected override string HostCore => "HostName";
        protected override string PeerCore => "PeerName";
        protected override DateTime DeadlineCore { get; }
        protected override Metadata RequestHeadersCore => _requestHeaders;
        protected override CancellationToken CancellationTokenCore => _cancellationToken;
        protected override Metadata ResponseTrailersCore => _responseTrailers;
        protected override Status StatusCore { get; set; }
        protected override WriteOptions? WriteOptionsCore { get => _writeOptions; set { _writeOptions = value; } }
        protected override AuthContext AuthContextCore => _authContext;

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
        {
            throw new NotImplementedException();
        }

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        {
            if (ResponseHeaders != null)
            {
                throw new InvalidOperationException("Response headers have already been written.");
            }

            ResponseHeaders = responseHeaders;
            return Task.CompletedTask;
        }

        protected override IDictionary<object, object> UserStateCore => _userState;

        public static TestServerCallContext Create(Metadata? requestHeaders = null, CancellationToken cancellationToken = default)
        {
            return new TestServerCallContext(requestHeaders ?? new Metadata(), cancellationToken);
        }
    }


    public class Tests
    {
        [Test]
        public async Task Test1()
        {
            TestServerCallContext testServerCallContext = TestServerCallContext.Create();
            Mock<IBusinessService> mockBusinessService = new Mock<IBusinessService>();
            GreeterService greeterService = new GreeterService(mockBusinessService.Object);
            HelloReply r = await greeterService.SayHello(new GrpcServiceUnderTest.HelloRequest { Name = "Gabor" }, testServerCallContext);
            Assert.That(r.Message, Is.EqualTo("Hello Gabor"), "response message incorrect - 1");

            TestAsyncStreamReader<HelloRequest> input = new TestAsyncStreamReader<HelloRequest>(testServerCallContext);
            input.AddMessage(new HelloRequest { Name = "Akos" });
            input.AddMessage(new HelloRequest { Name = "Akos2" });
            input.Complete();

            r = await greeterService.SayHelloClientStreaming(input, testServerCallContext);
            Assert.That(r.Message, Is.EqualTo("Gabor"), "response message incorrect - 2");
        }
    }
}