// See https://aka.ms/new-console-template for more information

using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcFileServer;

string filePath = "C:\\Users\\nagya\\Desktop\\20250319_165616.jpg";

using GrpcChannel channel = GrpcChannel.ForAddress("https://localhost:7278");
var client = new FileService.FileServiceClient(channel);


using AsyncClientStreamingCall<UploadFileRequest, UploadFileResponse> uploadCall = client.UploadFile();

await uploadCall.RequestStream.WriteAsync(new UploadFileRequest
{
    Metadata = new FileMetadata { Name = "kutya.jpg", Checksum = "ABC123", Length = 3 }
});

int chunkSize = 1024 * 300;
var buffer = new byte[chunkSize];
await using var readStream = File.OpenRead(filePath);

while (true)
{
    var count = await readStream.ReadAsync(buffer);
    if (count == 0)
        break;

    await uploadCall.RequestStream.WriteAsync(new UploadFileRequest
    {
        Data = UnsafeByteOperations.UnsafeWrap(buffer.AsMemory(0, count))
    });
}

await uploadCall.RequestStream.CompleteAsync();
UploadFileResponse resp = await uploadCall.ResponseAsync;

// download
using var downloadCall = client.DownloadFile(new DownloadFileRequest { FileId = resp.FileId });
await downloadCall.ResponseStream.MoveNext(CancellationToken.None);
FileMetadata metadata = downloadCall.ResponseStream.Current.Metadata;

using var writeStream = File.Create(metadata.Name);
await foreach (DownloadFileResponse response in downloadCall.ResponseStream.ReadAllAsync())
{
    await writeStream.WriteAsync(response.Data.Memory);
}

Console.ReadLine();