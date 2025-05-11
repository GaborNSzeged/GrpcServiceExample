namespace LoggerServer.Bll;

public interface IFileHandler
{
    Task<string> SaveContent(string folder, string fileName, string content);
}
