using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicTacToeGame.Data;
using TicTacToeGame.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<GamesDbContext>(options =>
{
    options.UseSqlite("Data Source=Database/TicTacToe.db");
});

builder.Services.AddScoped<IGameSessionsRepository, GameSessionRepository>();

builder.Services.AddDbContext<AppIdentityDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")!);
});
builder.Services.AddIdentity<GameUser, IdentityRole>().AddEntityFrameworkStores<AppIdentityDbContext>();

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

await SeedIdentityData.EnsurePopulated(app);

app.Run();
