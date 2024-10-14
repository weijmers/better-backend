using System.Globalization;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Core;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Importer;

public class ImportResultsEvent
{
    public string? CountryCode { get; set; }
    public int Division { get; set; }
    public string? Season { get; set; }
}

public class Results
{
    private string ResultsUrl(string season, string countryCode, string division) => $"https://www.football-data.co.uk/mmz4281/{season}/{countryCode}{division}.csv";

    private readonly IDdbRepository _ddbRepository;
    private readonly ILogger<Results> _logger;

    public Results()
    {
        var provider = Di.CreateServiceProvider();
        _ddbRepository = provider.GetRequiredService<IDdbRepository>();
        _logger = provider.GetRequiredService<ILogger<Results>>();
    }

    [LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
    public async Task Handler(ImportResultsEvent input, ILambdaContext context)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.CountryCode);
        if (input.Division < 1) throw new ArgumentException($"{nameof(input.Division)} missing or invalid");
        
        var season = input!.Season ?? await _ddbRepository.CurrentSeason(CancellationToken.None);
        var countryCode = input.CountryCode;
        var division = input.Division;
        
        using var client = new HttpClient();
        var resultsUrl = ResultsUrl(season, countryCode, TranslateDivision(countryCode, division));
        var response = await client.GetAsync(resultsUrl, CancellationToken.None);
        response.EnsureSuccessStatusCode();
        
        if (!await IsFileModified(resultsUrl, response.Content.Headers.ContentLength, CancellationToken.None))
        {
            _logger.LogInformation("File {URL} unchanged, no need to process again.", resultsUrl);
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
        csv.Context.RegisterClassMap<ResultMap>();

        foreach (var result in csv.GetRecords<Game>())
        {
            await _ddbRepository.SaveGame(result, CancellationToken.None);
        }
        
        await _ddbRepository.SaveImportedContentLength(resultsUrl, response.Content.Headers.ContentLength, CancellationToken.None);
    }

    private async Task<bool> IsFileModified(string url, long? contentLength, CancellationToken token)
    {
        var lastImportedContentLength = await _ddbRepository.GetImportedContentLength(url, token);
        return lastImportedContentLength != contentLength;
    }

    private static string TranslateDivision(string countryCode, int division)
    {
        if (countryCode == "E")
        {
            if (division == 5) return "C";
            return $"{division - 1}";
        }

        if (countryCode == "SC")
        {
            return $"{division - 1}";
        }
        
        return $"{division}";
    }
}