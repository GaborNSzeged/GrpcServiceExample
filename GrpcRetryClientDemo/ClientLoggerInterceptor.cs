﻿using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace GrpcRetryClientDemo;

//https://github.com/grpc/grpc-dotnet/blob/master/examples/Interceptor/Client/ClientLoggerInterceptor.cs
public class ClientLoggerInterceptor : Interceptor
    {
        private readonly ILogger<ClientLoggerInterceptor> _logger;

        public ClientLoggerInterceptor(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ClientLoggerInterceptor>();
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method);
            AddCallerMetadata(ref context);

            try
            {
                return continuation(request, context);
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method);
            AddCallerMetadata(ref context);

            try
            {
                var call = continuation(request, context);

                return new AsyncUnaryCall<TResponse>(HandleResponse(call.ResponseAsync), call.ResponseHeadersAsync, call.GetStatus, call.GetTrailers, call.Dispose);
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        private async Task<TResponse> HandleResponse<TResponse>(Task<TResponse> t)
        {
            try
            {
                var response = await t;
                _logger.LogInformation($"Response received: {response}");
                return response;
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method);
            AddCallerMetadata(ref context);

            try
            {
                return continuation(context);
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method);
            AddCallerMetadata(ref context);

            try
            {
                return continuation(request, context);
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method);
            AddCallerMetadata(ref context);

            try
            {
                return continuation(context);
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        private void LogCall<TRequest, TResponse>(Method<TRequest, TResponse> method)
            where TRequest : class
            where TResponse : class
        {
            _logger.LogInformation($"Starting call. Name: {method.Name}. Type: {method.Type}. Request: {typeof(TRequest)}. Response: {typeof(TResponse)}");
        }

        private void AddCallerMetadata<TRequest, TResponse>(ref ClientInterceptorContext<TRequest, TResponse> context)
            where TRequest : class
            where TResponse : class
        {
            var headers = context.Options.Headers;

            // Call doesn't have a headers collection to add to.
            // Need to create a new context with headers for the call.
            if (headers == null)
            {
                headers = new Metadata();
                var options = context.Options.WithHeaders(headers);
                context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);
            }

            // Add caller metadata to call headers
            headers.Add("caller-user", Environment.UserName);
            headers.Add("caller-machine", Environment.MachineName);
            headers.Add("caller-os", Environment.OSVersion.ToString());
        }

        private void LogError(Exception ex)
        {
            _logger.LogError(ex, $"Call error: {ex.Message}");
        }
    }
