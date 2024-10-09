using System.Text.Json;
using Core;
using Dapper;

namespace Api;

public class Controller(Repository repository)
{
    public async Task<IEnumerable<string>> GetCountries()
    {
        var connection = repository.GetConnection();
        var sql = "SELECT DISTINCT country_code FROM games;";
        var countries = await connection.QueryAsync<string>(sql);
        
        return countries;
    }
    
    public async Task<IEnumerable<Game>> GetFixtures()
    {
        var connection = repository.GetConnection();
        var sql = "SELECT * FROM games WHERE type = @Type;";
        var fixtureEntities = await connection.QueryAsync<GameEntity>(sql, new { Type = GameTypes.Fixture });

        return fixtureEntities
            .Select(entity => JsonSerializer.Deserialize<Game>(entity.Data))
            .OfType<Game>()
            .ToList();
    }
    
    public async Task<IEnumerable<Game>> GetFixturesByDate(DateTime date)
    {
        var connection = repository.GetConnection();
        var sql = "SELECT * FROM games WHERE date LIKE @Date || '%';";
        var fixtureEntities = await connection.QueryAsync<GameEntity>(sql, new { Type = GameTypes.Fixture, Date = $"{date:yyyy-MM-dd}" });
        
        return fixtureEntities
            .Select(entity => JsonSerializer.Deserialize<Game>(entity.Data))
            .OfType<Game>()
            .ToList();
    }
    
    public async Task<IEnumerable<Game>> GetGamesByTeamId(string teamId)
    {
        var connection = repository.GetConnection();
        var sql = "SELECT * FROM games WHERE home_team_id = @TeamId OR away_team_id = @TeamId;";
        var gameEntities = await connection.QueryAsync<GameEntity>(sql, new { TeamId = teamId });

        return gameEntities
            .Select(entity => JsonSerializer.Deserialize<Game>(entity.Data))
            .OfType<Game>()
            .ToList();
    }
    
}