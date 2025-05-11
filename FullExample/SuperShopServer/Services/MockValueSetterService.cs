using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using SuperShopServer.General;

namespace SuperShopServer.Services
{
    public class MockValueSetterService : MockValueSetter.MockValueSetterBase
    {
        public override Task<Empty> SetMockValue(SetMockValueReques request, ServerCallContext context)
        {
            if (request.ValueType == ValueType.Disk)
            {
                SystemInfo.Disk = request.Value;
            }
            else if (request.ValueType == ValueType.Cpu) 
            {
                SystemInfo.Cpu = request.Value;
            }
            else if (request.ValueType == ValueType.Memory)
            {
                SystemInfo.Memory = request.Value;
            }

            return Task.FromResult(new Empty());

        }
    }
}
