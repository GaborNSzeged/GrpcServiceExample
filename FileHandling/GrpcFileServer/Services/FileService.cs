using Google.Protobuf;
using Grpc.Core;

namespace GrpcFileServer.Services;

public class FileService : GrpcFileServer.FileService.FileServiceBase
{
    private readonly IFileRepository fileRepository;

    public FileService(IFileRepository fileRepository)
    {
        this.fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
    }

    public override async Task<UploadFileResponse> UploadFile(IAsyncStreamReader<UploadFileRequest> requestStream, ServerCallContext context)
    {
        if (!await requestStream.MoveNext())
            throw new Exception("No data received");

        FileMetadata fileMetadata = requestStream.Current.Metadata;
        (string id, string serverPath) = fileRepository.AddFileToDb();

        await using FileStream fileStream = File.Create(serverPath);
        await foreach (var request in requestStream.ReadAllAsync())
        {
            await fileStream.WriteAsync(request.Data.Memory);
        }

        // TODO: Checksum

        return new UploadFileResponse { FileId = id };
    }
    public override async Task DownloadFile(DownloadFileRequest request, IServerStreamWriter<DownloadFileResponse> responseStream, ServerCallContext context)
    {
        string serverFileName = fileRepository.GetFileFromDb(request.FileId);
        string checksum = "ABC123";
        await responseStream.WriteAsync(new DownloadFileResponse
        {
            Metadata = new FileMetadata { Checksum = checksum, Length = 3, Name="kutya.jpg" }
        });
        int chunkSize = 1024 * 3;
        var buffer = new byte[chunkSize];
        await using var fileStream = File.OpenRead(serverFileName);
        while (true)
        {
            var count = await fileStream.ReadAsync(buffer);
            if (count == 0)
                break;

            await responseStream.WriteAsync(new DownloadFileResponse
            {
                Data = UnsafeByteOperations.UnsafeWrap(buffer.AsMemory(0, count))
            });
        }
    }
}


public interface IFileRepository
{
    (string, string) AddFileToDb(); // + metadata (length, checksum, date, userId)
    string GetFileFromDb(string id);
}

public class FileRepository : IFileRepository
{
    private readonly Dictionary<string,string> fileIdPaths = new Dictionary<string,string>();
    public (string, string) AddFileToDb()
    {
        string id = Guid.NewGuid().ToString();
        string fileName = Guid.NewGuid().ToString();
        fileIdPaths.Add(id, fileName);
        return (id, fileName);
    }

    public string GetFileFromDb(string id)
    {
        return fileIdPaths[id];
    }
}
