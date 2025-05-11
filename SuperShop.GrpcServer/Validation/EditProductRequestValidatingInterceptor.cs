using FluentValidation.Results;
using Grpc.Core.Interceptors;

namespace SuperShop.GrpcServer.Validation
{
    public class EditProductRequestValidatingInterceptor : Interceptor
    {
        // rcp hívás előtt fog meghívódik
        // ha meghívódott az rpc utána visszatérünk ide az Invoke után
        // UnaryServerMethod -> delegate which represents the rcp call
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            // this could be a general solution. The interceptors could be find in DI container.
            // builder.Services.AddTransient<AbstractValidator<EditProductRequest>, EditProductResponseValidator>>();

            if (request is EditProductRequest editProductRequest)
            {
                EditProductRequestValidator validator = new();
                ValidationResult validationResult = await validator.ValidateAsync(editProductRequest);

                if (!validationResult.IsValid)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument,  $"Error: {validationResult}"));
                }
            }

            TResponse resp = await continuation.Invoke(request, context);
            return resp;
        }

        //public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream,
        //    ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
        //{
        //    return base.ServerStreamingServerHandler(request, responseStream, context, continuation);

        //}

        public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context, ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            // cannot validate the stream here on the spot but when the rpc will call the next metohod of the requestStream
            return base.ClientStreamingServerHandler(new ValidatingAsyncStreamReader<TRequest>(requestStream), context, continuation);
        }
    }   
}
