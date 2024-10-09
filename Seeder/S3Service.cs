using Amazon.S3;
using Amazon.S3.Transfer;

namespace Seeder;

public interface IS3Service
{
    Task UploadAsync(string filePath, string key, CancellationToken token);
}

public class S3Service(IAmazonS3 client) : IS3Service
{
    public async Task UploadAsync(string filePath, string key, CancellationToken token)
    {
        var fileTransferUtility = new TransferUtility(client);
        await fileTransferUtility.UploadAsync(filePath, Environment.GetEnvironmentVariable("BUCKET"), key, token);
    }
}