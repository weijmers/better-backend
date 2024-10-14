using System.Globalization;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Core;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Importer;

public class Fixtures
{
    private const string FixturesUrl = "https://www.football-data.co.uk/fixtures.csv";

    private readonly IDdbRepository _ddbRepository;
    private readonly ILogger<Fixtures> _logger;

    public Fixtures()
    {
        var provider = Di.CreateServiceProvider();
        _ddbRepository = provider.GetRequiredService<IDdbRepository>();
        _logger = provider.GetRequiredService<ILogger<Fixtures>>();
    }

    [LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
    public async Task Handler(object input, ILambdaContext context)
    {
        using var client = new HttpClient();
        var response = await client.GetAsync(FixturesUrl, CancellationToken.None);
        response.EnsureSuccessStatusCode();
        
        if (!await IsFileModified(FixturesUrl, response.Content.Headers.ContentLength, CancellationToken.None))
        {
            _logger.LogInformation("File {URL} unchanged, no need to process again.", FixturesUrl);
            return;
        }
        
        using var reader = new StreamReader(await response.Content.ReadAsStreamAsync(CancellationToken.None));
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            MissingFieldFound = null,
            BadDataFound = args =>
            {
                _logger.LogError("Error on {Record}", args.RawRecord);
            }
        });
        csv.Context.RegisterClassMap<FixtureMap>();

        foreach (var fixture in csv.GetRecords<Game>())
        {
            await _ddbRepository.SaveGame(fixture, CancellationToken.None);
        }
        
        await _ddbRepository.SaveImportedContentLength(FixturesUrl, response.Content.Headers.ContentLength, CancellationToken.None);
    }

    private async Task<bool> IsFileModified(string url, long? contentLength, CancellationToken token)
    {
        var lastImportedContentLength = await _ddbRepository.GetImportedContentLength(url, token);
        return lastImportedContentLength != contentLength;
    }
}