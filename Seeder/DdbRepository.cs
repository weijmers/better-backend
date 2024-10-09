using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Core;
using Microsoft.Extensions.Logging;

namespace Seeder;

public interface IDdbRepository
{
    Task<List<Game>> GetGames(CancellationToken token);
}

public class DdbRepository(IAmazonDynamoDB db, ILogger<DdbRepository> logger) : IDdbRepository
{
    private readonly IDynamoDBContext _context = new DynamoDBContext(db);
    
    public async Task<List<Game>> GetGames(CancellationToken token)
    {
        var games = new List<Game>();
        var search = _context.ScanAsync<Game>([], new DynamoDBOperationConfig
        {
            OverrideTableName = Environment.GetEnvironmentVariable("GAMES_TABLE")
        });
        
        while (!search.IsDone)
        {
            var response = await search.GetNextSetAsync(token);
            if (response?.Count > 0)
            {
                games.AddRange(response);
            }
        }

        return games;
    }
}