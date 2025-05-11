using Grpc.Core.Interceptors;

namespace SuperShop.GrpcServer.ErrorHandling;

public class ErrorHandlingInterceptor : Interceptor
{
    private readonly IErrorHandler errorHandler;
    //private readonly IBusinessErrorHandler businessErrorHandler;

    public ErrorHandlingInterceptor(IErrorHandler errorHandler)
    {
        this.errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation.Invoke(request, context);
        }
        //catch (Exception ex) when (businessErrorHandler.CanHandle(ex))
        //{
        //    await businessErrorHandler.HandleError(ex);
        //}
        catch (SuperShopBusinessException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (Exception ex) when (ex is not RpcException)
        {
            string correlationId = Guid.NewGuid().ToString();
            await errorHandler.HandleError(ex, correlationId);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"),
                                   new Metadata { { "CorrelationId", correlationId } });
        }
    }

    // TODO: client,server, duplex
}

