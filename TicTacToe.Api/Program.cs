using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicTacToe.Api.Data;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;
using TicTacToe.Api.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

if (builder.Environment.IsProduction())
{
    builder.Services.AddDbContext<GamesDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("GamesConnection")!, sqlServerOptions =>
        {
            sqlServerOptions.EnableRetryOnFailure();
        });
    });

    builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")!, sqlServerOptions =>
        {
            sqlServerOptions.EnableRetryOnFailure();
        });
    });
}
else
{
    builder.Services.AddDbContext<GamesDbContext>(options =>
    {
        options.UseInMemoryDatabase("GamesDb");
    });
    
    builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    {
        options.UseInMemoryDatabase("IdentityDb");
    });
}

builder.Services.AddIdentity<GameUser, IdentityRole>().AddEntityFrameworkStores<AppIdentityDbContext>();

builder.Services.AddScoped<IGameSessionsRepository, GameSessionRepository>();
builder.Services.AddHostedService<GamesDataHostedService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<GamesHub>("/hub/games");

PrepareGamesDatabase.PrepareDatabase(app, app.Environment);
await PrepareIdentityDatabase.EnsurePopulated(app, app.Environment);

app.Run();
