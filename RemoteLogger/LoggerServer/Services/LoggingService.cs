using Grpc.Core;
using LoggerServer.Bll;

namespace LoggerServer.Services;

public class LoggingService : Logging.LoggingBase
{
    private readonly ILogger<LoggingService> _logger;
    private IFileHandler _fileHandler;

    public LoggingService(IFileHandler fileHandler, ILogger<LoggingService> logger)
    {
        _fileHandler = fileHandler;
        _logger = logger;
    }

    public override async Task<SendContentReply> SendContent(SendContentRequest request, ServerCallContext context)
    {
        Log($"SendContent: Client: {request.ClientName}, file name: {request.FileName}");
        string error = await _fileHandler.SaveContent(request.ClientName, request.FileName, request.Content);
        string responseMessage = string.Empty;

        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogError($"SendContent: Client:{request.ClientName}, File:{request.FileName} {Environment.NewLine} {error}");
            responseMessage = error;
        }

        return new SendContentReply { Message = responseMessage };
    }

    private void Log(string message)
    {
        _logger.LogInformation(message);
    }
}
