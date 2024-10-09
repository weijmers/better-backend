using Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);
builder.Services.AddSingleton<Repository>(_ => new Repository("Data Source=/tmp/database.db;Version=3;"));
builder.Services.AddSingleton<Controller>();

var app = builder.Build();
var controller = app.Services.GetRequiredService<Controller>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "Root path ...!");
app.MapGet("/countries", controller.GetCountries);
app.MapGet("/fixtures", controller.GetFixtures);
app.MapGet("/fixtures/{date:datetime}", controller.GetFixturesByDate);
app.MapGet("/games", () => "All games ...?");
app.MapGet("/games/{teamId}", controller.GetGamesByTeamId);

app.Run();