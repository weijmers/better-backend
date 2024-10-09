using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Core;
using Microsoft.Extensions.Logging;

namespace Importer;

public interface IDdbRepository
{
    Task<string> CurrentSeason(CancellationToken token);
    Task SaveGame(Game game, CancellationToken token);
    Task SaveImportedContentLength(string url, long? contentLength, CancellationToken token);
    Task<long?> GetImportedContentLength(string url, CancellationToken token);
}

public class DdbRepository(IAmazonDynamoDB db, ILogger<DdbRepository> logger) : IDdbRepository
{
    private readonly IDynamoDBContext _context = new DynamoDBContext(db);

    public async Task SaveGame(Game game, CancellationToken token)
    {
        var document = _context.ToDocument(game);
        var attributeValues = document.ToAttributeMap();
        
        var request = new PutItemRequest
        {
            TableName = Environment.GetEnvironmentVariable("GAMES_TABLE"),
            Item = attributeValues,
        };

        switch (game.Type)
        {
            case GameTypes.Fixture:
                request.ConditionExpression = "attribute_not_exists(#id) AND attribute_not_exists(#date)";
                request.ExpressionAttributeNames = new()
                {
                    { "#id", "Id" },
                    { "#date", "Date" },
                };
                break;
            case GameTypes.Result:
                request.ConditionExpression = "(attribute_not_exists(#id) AND attribute_not_exists(#date)) OR #type = :fixture";
                request.ExpressionAttributeNames = new()
                {
                    { "#id", "Id" },
                    { "#date", "Date" },
                    { "#type", "Type" },
                };
                request.ExpressionAttributeValues = new()
                {
                    { ":fixture", new AttributeValue { S = GameTypes.Fixture } }
                };
                break;
            default: 
                throw new NotImplementedException($"Game type {game.Type} is not implemented.");
        }

        try
        {
            await db.PutItemAsync(request, token);
        }
        catch (ConditionalCheckFailedException)
        {
            logger.LogInformation("Ignore ..., game already saved as a result: {Id}", game.Id);
        }
    }
    
    

    public async Task SaveImportedContentLength(string url, long? contentLength, CancellationToken token)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "PK", new AttributeValue { S = "IMPORT" } },
            { "SK", new AttributeValue { S = url } },
            { "ContentLength", new AttributeValue { N = $"{contentLength ?? 0}" } },
            { "ModifiedAt", new AttributeValue { S = $"{DateTimeOffset.UtcNow:o}" } },
        };
        
        var request = new PutItemRequest
        {
            TableName = Environment.GetEnvironmentVariable("SETTINGS_TABLE"),
            Item = item,
        };
        
        await db.PutItemAsync(request, token);
    }


    public async Task<long?> GetImportedContentLength(string url, CancellationToken token)
    {
        var request = new GetItemRequest
        {
            TableName = Environment.GetEnvironmentVariable("SETTINGS_TABLE"),
            Key = new()
            {
                { "PK", new AttributeValue { S = "IMPORT" } },
                { "SK", new AttributeValue { S = url } },
            }
        };
        
        var response = await db.GetItemAsync(request, token);
        if (response.Item.Count > 0)
        {
            return long.Parse(response.Item["ContentLength"].N);
        }
        
        return null;
    }

    public async Task<string> CurrentSeason(CancellationToken token)
    {
        var request = new QueryRequest
        {
            TableName = Environment.GetEnvironmentVariable("SETTINGS_TABLE"),
            KeyConditionExpression = "#pk = :pk",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#pk", "PK" },
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":pk", new AttributeValue { S = "SEASON" } },
            },
            ScanIndexForward = false,
            Limit = 1,
        };

        var response = await db.QueryAsync(request, token);
        if (response.Count == 0)
        {
            throw new NullReferenceException("No items found");
        }

        return response.Items[0]["SK"].S;
    }
}