using Amazon.DynamoDBv2;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Importer;

public static class Di
{
    public static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        
        services.AddSingleton<IAmazonS3>(new AmazonS3Client());
        services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient());
        
        services.AddScoped<IS3Service, S3Service>();
        services.AddScoped<IDdbRepository, DdbRepository>();

        return services.BuildServiceProvider();
    }
}