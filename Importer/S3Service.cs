using System.Globalization;
using System.Net;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;

namespace Importer;

public interface IS3Service
{
    Task<(string currentId, string previousId)> GetVersions(string key, CancellationToken token);
    Task<string?> DownloadAsync(string key, string versionId, CancellationToken token);
}

public class S3Service(IAmazonS3 client) : IS3Service
{
    public async Task<(string currentId, string previousId)> GetVersions(string key, CancellationToken token)
    {
        var request = new ListVersionsRequest
        {
            BucketName = Environment.GetEnvironmentVariable("BUCKET"),
            Prefix = key,
        };
        
        var response = await client.ListVersionsAsync(request, token);
        var versions = response.Versions
            .Where(v => !v.IsDeleteMarker)
            .OrderBy(v => v.LastModified)
            .ToList();
        
        Console.WriteLine("Versions: " + string.Join(", ", versions.Select(v => v.VersionId)));
        
        var currentId = versions.Count > 0 ? versions[0].VersionId : string.Empty;
        var previousId = versions.Count > 1 ? versions[1].VersionId : string.Empty;
        
        return (currentId, previousId);
    }
    
    public async Task<string?> DownloadAsync(string key, string versionId, CancellationToken token)
    {
        var request = new GetObjectRequest
        {
            BucketName = Environment.GetEnvironmentVariable("BUCKET"),
            Key = key,
            VersionId = versionId,
        };

        try
        {
            var response = await client.GetObjectAsync(request, token);
        
            await using var responseStream = response.ResponseStream;
            using var reader = new StreamReader(responseStream);
            var contents = await reader.ReadToEndAsync(token);
        
            return contents;
        }
        catch (Exception e)
        {
            return string.Empty;
        }
    }
}