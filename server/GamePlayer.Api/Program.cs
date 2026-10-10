using GamePlayer.Api.Data;
using GamePlayer.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Register controllers
builder.Services.AddControllers();

// Register OpenAPI
builder.Services.AddOpenApi();

// Read the database connection string
var connectionString =
    builder.Configuration.GetConnectionString(
        "GamePlayerDatabase"
    )
    ?? throw new InvalidOperationException(
        "Database connection string was not found."
    );

// Register AppDbContext and PostgreSQL
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseNpgsql(connectionString)
);

builder.Services.AddScoped<
    ILeaderboardService,
    LeaderboardService
>();

builder.Services.AddScoped<
    IMatchService,
    MatchService
>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
