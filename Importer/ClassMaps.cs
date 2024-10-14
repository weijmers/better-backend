using System.Globalization;
using Core;
using CsvHelper;
using CsvHelper.Configuration;

namespace Importer;

public sealed class FixtureMap : GameMap
{
    public FixtureMap()
    {
        Map(m => m.Type).Constant(GameTypes.Fixture);
        Map(m => m.Expiration).Convert(ConvertExpiration);
    }
    
    private static long ConvertExpiration(ConvertFromStringArgs args)
    {
        var dateTime = ConvertDateTime(args);
        return ((DateTimeOffset)dateTime.AddDays(7)).ToUnixTimeSeconds();
    }
}

public sealed class ResultMap : GameMap
{
    public ResultMap()
    {
        Map(m => m.Type).Constant(GameTypes.Result);
        Map(m => m.Expiration).Convert(ConvertExpiration);
    }

    private static long ConvertExpiration(ConvertFromStringArgs args)
    {
        var dateTime = ConvertDateTime(args);
        return ((DateTimeOffset)dateTime.AddYears(2)).ToUnixTimeSeconds();
    }
}

public abstract class GameMap : ClassMap<Game>
{
    public GameMap()
    {
        Map(m => m.CountryCode).Convert(ConvertCountryCode);
        Map(m => m.Division).Convert(ConvertDivision);
        Map(m => m.Date).Convert(ConvertDateTime);
        Map(m => m.HomeTeam).Name("HomeTeam");
        Map(m => m.AwayTeam).Name("AwayTeam");
        Map(m => m.Referee).Name("Referee").Optional();
        Map(m => m.Result).Name("FTR").Optional();
        Map(m => m.HalftimeResult).Name("HTR").Optional();
        Map(m => m.Odds).Convert(ConvertOdds);
        
        Map(m => m.ModifiedAt).Constant(DateTime.UtcNow);

        References<StatisticsMap>(m => m.Statistics);
    }

    private static string ConvertCountryCode(ConvertFromStringArgs args)
    {
        var countryAndDivision = args.Row.GetField("Div") ?? throw new ArgumentException("Missing {Div}.");
        if (countryAndDivision is "") return "";
        
        var countryCode = countryAndDivision[..^1];
        return countryCode;
    }

    private static int ConvertDivision(ConvertFromStringArgs args)
    {
        var countryAndDivision = args.Row.GetField("Div") ?? throw new ArgumentException("Missing {Div}.");
        if (countryAndDivision is "") return 0;
        
        var countryCode = countryAndDivision[..^1];
        var division = countryAndDivision[^1];

        int.TryParse($"{division}", out var parsedDivision);
        
        if (!char.IsDigit(division))
        {
            parsedDivision = 4;
        }

        if (countryCode is "E" or "SC")
        {
            parsedDivision += 1;
        }

        return parsedDivision;
    }
    
    protected static DateTime ConvertDateTime(ConvertFromStringArgs args)
    {
        var date = args.Row.GetField("Date") ?? throw new ArgumentException("Missing {Date}.");
        var time = args.Row.GetField("Time") ?? "00:00";
        if (date is "") return DateTime.MinValue;
        
        if (DateTime.TryParseExact($"{date} {time}", "dd/MM/yyyy HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal,
                out DateTime fourDigitYear))
        {
            return fourDigitYear;
        }

        if (DateTime.TryParseExact($"{date} {time}", "dd/MM/yy HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal,
                out DateTime twoDigitYear))
        {
            return twoDigitYear;
        }

        return DateTime.MinValue;
    }
    
    private static List<Odds> ConvertOdds(ConvertFromStringArgs args)
    {
        List<Odds> odds = 
        [
            new()
            {
                Company = "Bet365",
                HomeWin = args.Row.GetField<double?>("B365H"),
                Draw = args.Row.GetField<double?>("B365D"),
                AwayWin = args.Row.GetField<double?>("B365A"),
            },
            new()
            {
                Company = "Betfair",
                HomeWin = args.Row.GetField<double?>("BFH"),
                Draw = args.Row.GetField<double?>("BFD"),
                AwayWin = args.Row.GetField<double?>("BFA"),
            },
            new()
            {
                Company = "Blue Square",
                HomeWin = args.Row.GetField<double?>("BSH"),
                Draw = args.Row.GetField<double?>("BSD"),
                AwayWin = args.Row.GetField<double?>("BSA"),
            },
            new()
            {
                Company = "Bet&Win",
                HomeWin = args.Row.GetField<double?>("BWH"),
                Draw = args.Row.GetField<double?>("BWD"),
                AwayWin = args.Row.GetField<double?>("BWA"),
            },
            new()
            {
                Company = "Gamebookers",
                HomeWin = args.Row.GetField<double?>("GBH"),
                Draw = args.Row.GetField<double?>("GBD"),
                AwayWin = args.Row.GetField<double?>("GBA"),
            },
            new()
            {
                Company = "Interwetten",
                HomeWin = args.Row.GetField<double?>("IWH"),
                Draw = args.Row.GetField<double?>("IWD"),
                AwayWin = args.Row.GetField<double?>("IWA"),
            },
            new()
            {
                Company = "Ladbrokes",
                HomeWin = args.Row.GetField<double?>("LBH"),
                Draw = args.Row.GetField<double?>("LBD"),
                AwayWin = args.Row.GetField<double?>("LBA"),
            },
            new()
            {
                Company = "Pinnacle",
                HomeWin = args.Row.GetField<double?>("PSH"),
                Draw = args.Row.GetField<double?>("PSD"),
                AwayWin = args.Row.GetField<double?>("PSA"),
            },
            new()
            {
                Company = "Sporting Odds",
                HomeWin = args.Row.GetField<double?>("SOH"),
                Draw = args.Row.GetField<double?>("SOD"),
                AwayWin = args.Row.GetField<double?>("SOA"),
            },
            new()
            {
                Company = "Sportingbet",
                HomeWin = args.Row.GetField<double?>("SBH"),
                Draw = args.Row.GetField<double?>("SBD"),
                AwayWin = args.Row.GetField<double?>("SBA"),
            },
            new()
            {
                Company = "Stan James",
                HomeWin = args.Row.GetField<double?>("SJH"),
                Draw = args.Row.GetField<double?>("SJD"),
                AwayWin = args.Row.GetField<double?>("SJA"),
            },
            new()
            {
                Company = "Stanleybet",
                HomeWin = args.Row.GetField<double?>("SYH"),
                Draw = args.Row.GetField<double?>("SYD"),
                AwayWin = args.Row.GetField<double?>("SYA"),
            },
            new()
            {
                Company = "VC Bet",
                HomeWin = args.Row.GetField<double?>("VCH"),
                Draw = args.Row.GetField<double?>("VCD"),
                AwayWin = args.Row.GetField<double?>("VCA"),
            },
            new()
            {
                Company = "William Hill",
                HomeWin = args.Row.GetField<double?>("WHH"),
                Draw = args.Row.GetField<double?>("WHD"),
                AwayWin = args.Row.GetField<double?>("WHA"),
            }
        ];

        return odds
            .Where(o => o is { HomeWin: not null, Draw: not null, AwayWin: not null })
            .ToList();
    }
}

public sealed class StatisticsMap : ClassMap<Statistics>
{
    public StatisticsMap()
    {
        Map(m => m.Attendance).Name("Attendance").Optional();

        // Home team statistics
        Map(m => m.HomeGoals).Name("FTHG").Optional();
        Map(m => m.HalftimeHomeGoals).Name("HTHG").Optional();
        Map(m => m.HomeShots).Name("HS").Optional();
        Map(m => m.HomeShotsOnTarget).Name("HST").Optional();
        Map(m => m.HomeHitWoodwork).Name("HHW").Optional();
        Map(m => m.HomeCorners).Name("HC").Optional();
        Map(m => m.HomeFoulsCommitted).Name("HF").Optional();
        Map(m => m.HomeFreeKicksConceded).Name("HFKC").Optional();
        Map(m => m.HomeOffsides).Name("HO").Optional();
        Map(m => m.HomeYellowCards).Name("HY").Optional();
        Map(m => m.HomeRedCards).Name("HR").Optional();
        Map(m => m.HomeBookingsPoints).Name("HBP").Optional();

        // Away team statistics
        Map(m => m.AwayGoals).Name("FTAG").Optional();
        Map(m => m.HalftimeAwayGoals).Name("HTAG").Optional();
        Map(m => m.AwayShots).Name("AS").Optional();
        Map(m => m.AwayShotsOnTarget).Name("AST").Optional();
        Map(m => m.AwayHitWoodwork).Name("AHW").Optional();
        Map(m => m.AwayCorners).Name("AC").Optional();
        Map(m => m.AwayFoulsCommitted).Name("AF").Optional();
        Map(m => m.AwayFreeKicksConceded).Name("AFKC").Optional();
        Map(m => m.AwayOffsides).Name("AO").Optional();
        Map(m => m.AwayYellowCards).Name("AY").Optional();
        Map(m => m.AwayRedCards).Name("AR").Optional();
        Map(m => m.AwayBookingsPoints).Name("ABP").Optional();
    }
}