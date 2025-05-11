namespace SuperShop.GrpcServer.Validation;

public class ValidatingAsyncStreamReader<TRequest> : IAsyncStreamReader<TRequest>
{
    private IAsyncStreamReader<TRequest> _reader;

    public ValidatingAsyncStreamReader(IAsyncStreamReader<TRequest> reader)
    {
        _reader = reader;
    }

    public TRequest Current => _reader.Current;

    public async Task<bool> MoveNext(CancellationToken cancellationToken)
    {
        if (await _reader.MoveNext(cancellationToken))
        {
            if (Current is EditProductRequest editProductRequest)
            {
                EditProductRequestValidator validator = new EditProductRequestValidator();
                FluentValidation.Results.ValidationResult validationResult = await validator.ValidateAsync(editProductRequest);

                if (!validationResult.IsValid)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Error"));
                }
            }
            return true;
        }
        return false;
    }
}

