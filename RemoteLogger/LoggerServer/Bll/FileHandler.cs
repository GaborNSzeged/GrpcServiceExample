namespace LoggerServer.Bll;

public class FileHandler : IFileHandler
{
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
    private ISettings _settings;

    public FileHandler(ISettings settings)
    {
        _settings = settings;
    }

    public async Task<string> SaveContent(string clientName, string fileName, string content)
    {
        if (string.IsNullOrEmpty(clientName))
        {
            clientName = "unknowClient";
        }

        string folderPath = Path.Combine($"{_settings.LogDirectory}", clientName);

        try
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, fileName);

            await _semaphoreSlim.WaitAsync();
            try
            {
                if (File.Exists(filePath))
                {
                    await File.AppendAllTextAsync(filePath, content);
                }
                else
                {
                    await File.WriteAllTextAsync(filePath, content);
                }
            }
            finally
            {
                _semaphoreSlim?.Release();
            }
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
        return string.Empty;
    }
}
