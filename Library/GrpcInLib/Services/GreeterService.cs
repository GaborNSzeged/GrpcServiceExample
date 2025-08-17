using Grpc.Core;
using GrpcService1;

namespace GrpcInLib.Services
{
    internal class GreeterService : Greeter.GreeterBase
    {
        private MbdParameters _options;

        public GreeterService(MbdParameters options)
        {
            _options = options;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            var response = new HelloReply() { Message = $"Hello {request.Name}, welcome to service world." };
            return Task.FromResult(response);
        }
    }
}
