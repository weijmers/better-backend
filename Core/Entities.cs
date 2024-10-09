namespace Core;

public static class GameTypes
{
    public const string Fixture = "FIXTURE";
    public const string Result = "RESULT";
}

public static class GameResult
{
    public const string Home = "H";
    public const string Draw = "D";
    public const string Away = "A";
}

public class GameEntity
{
    public string Id { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; }
    public string Data { get; set; }
}

public class Game
{
    private string? _id;
    public string Id
    {
        get => $"{CountryCode.Slugify()}#{HomeTeam.Slugify()}#{AwayTeam.Slugify()}";
        set => _id = value;
    }
    public required string Type { get; set; }
    public required string CountryCode { get; set; }
    public int Division { get; set; }
    public DateTime Date { get; set; }
    public string? Referee { get; set; }
    public required string HomeTeam { get; set; }
    
    private string? _homeTeamId;
    public string HomeTeamId
    {
        get => $"{HomeTeam.Slugify()}";
        set => _homeTeamId = value;
    }
    public required string AwayTeam { get; set; }
    
    private string? _awayTeamId;
    public string AwayTeamId
    {
        get => $"{AwayTeam.Slugify()}";
        set => _awayTeamId = value;
    }
    
    public string? Result { get; set; }
    public string? HalftimeResult { get; set; }
    
    public Statistics? Statistics { get; set; }
    public List<Odds>? Odds { get; set; }
    public DateTime ModifiedAt { get; set; }
    public long Expiration { get; set; }
}

public class Statistics
{
    public int? Attendance { get; set; }
    public int? HomeGoals { get; set; }
    public int? AwayGoals { get; set; }
    public int? HalftimeHomeGoals { get; set; }
    public int? HalftimeAwayGoals { get; set; }
    public int? HomeShots { get; set; }
    public int? AwayShots { get; set; }
    public int? HomeShotsOnTarget { get; set; }
    public int? AwayShotsOnTarget { get; set; }
    public int? HomeHitWoodwork { get; set; }
    public int? AwayHitWoodwork { get; set; }
    public int? HomeCorners { get; set; }
    public int? AwayCorners { get; set; }
    public int? HomeFoulsCommitted { get; set; }
    public int? AwayFoulsCommitted { get; set; }
    public int? HomeFreeKicksConceded { get; set; }
    public int? AwayFreeKicksConceded { get; set; }
    public int? HomeOffsides { get; set; }
    public int? AwayOffsides { get; set; }
    public int? HomeYellowCards { get; set; }
    public int? AwayYellowCards { get; set; }
    public int? HomeRedCards { get; set; }
    public int? AwayRedCards { get; set; }
    public int? HomeBookingsPoints { get; set; } // 10 for yellow, 25 for red
    public int? AwayBookingsPoints { get; set; } // 10 for yellow, 25 for red
}

public class Odds
{
    public string? Company { get; set; }
    public double? HomeWin { get; set; }
    public double? Draw { get; set; }
    public double? AwayWin { get; set; }
}