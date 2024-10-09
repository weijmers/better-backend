using System.Data.SQLite;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace Seeder;

public class Seeder
{
    private const string DbFilePath = "/tmp/database.db";

    private readonly IS3Service _s3Service;
    private readonly IDdbRepository _ddbRepository;

    public Seeder()
    {
        var provider = Di.CreateServiceProvider();
        _s3Service = provider.GetRequiredService<IS3Service>();
        _ddbRepository = provider.GetRequiredService<IDdbRepository>();
    }

    [LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
    public async Task Handler(object input, ILambdaContext context)
    {
        await using (var connection = new SQLiteConnection($"Data Source={DbFilePath};Version=3;"))
        {
            connection.Open();
            connection.Execute("DROP TABLE IF EXISTS games;");
            
            var createTableQuery = @"
                CREATE TABLE games (
                    id TEXT NOT NULL,
                    date TEXT NOT NULL,
                    data TEXT NOT NULL,
                    type GENERATED ALWAYS AS (json_extract(data, '$.Type')) STORED,
                    country_code GENERATED ALWAYS AS (json_extract(data, '$.CountryCode')) STORED,
                    home_team_id GENERATED ALWAYS AS (json_extract(data, '$.HomeTeamId')) STORED,
                    away_team_id GENERATED ALWAYS AS (json_extract(data, '$.AwayTeamId')) STORED,
                    PRIMARY KEY (id, date)
                );";
            connection.Execute(createTableQuery);
            connection.Execute("CREATE INDEX IF NOT EXISTS idx_date ON games (date);");
            connection.Execute("CREATE INDEX IF NOT EXISTS idx_type ON games (type);");
            connection.Execute("CREATE INDEX IF NOT EXISTS idx_country_code ON games (country_code);");
            connection.Execute("CREATE INDEX IF NOT EXISTS idx_home_team_id ON games (away_team_id);");
            connection.Execute("CREATE INDEX IF NOT EXISTS idx_away_team_id ON games (away_team_id);");

            var games = await _ddbRepository.GetGames(CancellationToken.None);
            connection.Execute("INSERT INTO games (id, date, data) VALUES (@Id, @Date, @Data);",
                games.Select(
                    g => new
                    {
                        g.Id,
                        g.Date,
                        Data = JsonSerializer.Serialize(g)
                    }));
        }

        await _s3Service.UploadAsync(DbFilePath, "database.db", CancellationToken.None);
    }
}