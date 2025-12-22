using Google.Protobuf.MBD;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MbdExperiment.Services;

namespace MbdExperiment.Services;

    public class MBDServiceService : MBDService.MBDServiceBase
    {
        private readonly ILogger<GreeterService> _logger;
        public MBDServiceService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<Empty> AttachSessionFromFile(StringValue request, ServerCallContext context)
        {
            return base.AttachSessionFromFile(request, context);
        }

        public override Task<Empty> AttachSessionFromStream(BytesValue request, ServerCallContext context)
        {
            return base.AttachSessionFromStream(request, context);
        }
    }
