// See https://aka.ms/new-console-template for more information

using Google.Protobuf;
using Google.Protobuf.MBD;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Semilab.MBD.MBDCommunication.Google.Protobuf.MBDCalculator;

Console.WriteLine("Hello, World!");

var l = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

using GrpcChannel grpcChannel = GrpcChannel.ForAddress("https://localhost:7221");

MBDService.MBDServiceClient clientMbd = new MBDService.MBDServiceClient(grpcChannel);
MbdRcwaCalculatorService.MbdRcwaCalculatorServiceClient rcwClientCalc = new MbdRcwaCalculatorService.MbdRcwaCalculatorServiceClient(grpcChannel);

// Load session from file
using AsyncUnaryCall<Empty> call = clientMbd.AttachSessionFromFileAsync(new StringValue { Value = "session_file_path" });
Empty empty = await call.ResponseAsync;

// Load session from stream
using AsyncUnaryCall<Empty> call2 = clientMbd.AttachSessionFromStreamAsync(new BytesValue { Value = ByteString.CopyFromUtf8("session_stream_data") });
Empty empty2 = await call2.ResponseAsync;

// GetParameters
using AsyncUnaryCall<GetParametersResult> call3 = rcwClientCalc.GetParametersAsync(new GetParametersRequest());
GetParametersResult parametersResult = await call3.ResponseAsync;


Console.WriteLine("Parameters received:");
foreach (var parameter in parametersResult.Parameters)
{
    Console.WriteLine($"Parameter Name: {parameter.Name}, Value: {parameter.Value}");
}

// SetParameters
var setParametersRequest = new SetParametersRequest();
setParametersRequest.Parameters.Add(new ParameterItem { Name = "TCD", Value = 123 });
using AsyncUnaryCall<SetParametersResult> call4 = rcwClientCalc.SetParametersAsync(setParametersRequest);
SetParametersResult setParametersResult = await call4.ResponseAsync;
if (setParametersResult.Errors.Any())
{
   Console.WriteLine($"Errors occurred while setting parameters: {string.Concat(setParametersResult.Errors)}");
}
else
{
    Console.WriteLine("Params set OK.");
}




Console.ReadLine();
