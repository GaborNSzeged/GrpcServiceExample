namespace SuperShop.GrpcServer.ErrorHandling
{
    //public interface IBusinessErrorHandler
    //{
    //    Task HandleError(Exception ex);
    //    bool CanHandle(Exception ex);
    //}

    public interface IErrorHandler
    {
        Task HandleError(Exception ex, string correlationId);
    }

}
