using Microsoft.EntityFrameworkCore;

namespace TicTacToe.Api.Data
{
    public static class PrepareGamesDatabase
    {
        public static void PrepareDatabase(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            if (environment.IsProduction())
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<GamesDbContext>();
                    
                    if (context.Database.GetPendingMigrations().Any())
                    {
                        context.Database.Migrate();
                    }
                }
            }
        }
    }
}