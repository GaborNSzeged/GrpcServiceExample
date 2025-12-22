using Grpc.Core;
using Semilab.MBD.MBDCommunication.Google.Protobuf.MBDCalculator;

namespace MbdExperiment.Services;

public class MbdRcwCalculatorService : MbdRcwaCalculatorService.MbdRcwaCalculatorServiceBase
{
    private readonly ILogger<MbdRcwCalculatorService> _logger;

    public MbdRcwCalculatorService(ILogger<MbdRcwCalculatorService> logger)
    {
        _logger = logger;
    }

    public override Task<GetParametersResult> GetParameters(GetParametersRequest request, ServerCallContext context)
    {
        var result = new GetParametersResult();
        result.Parameters.Add(new ParameterItem
        {
            Name = "ExampleParameter",
            Value = 333
        });

        return Task.FromResult(result);
    }

    public override Task<SetParametersResult> SetParameters(SetParametersRequest request, ServerCallContext context)
    {
        SetParametersResult setParametersResult = new();
        return Task.FromResult(setParametersResult);
    }

    public override Task<FullSpectrumCalculationResult> FullSpectrumCalculation(FullSpectrumCalculationRequest request, ServerCallContext context)
    {
        return base.FullSpectrumCalculation(request, context);
    }
}

