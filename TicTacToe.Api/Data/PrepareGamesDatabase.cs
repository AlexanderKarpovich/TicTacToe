using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace TicTacToe.Api.Data
{
    public class PrepareGamesDatabase
    {
        public static void PrepareDatabase(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            if (environment.IsProduction())
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<GamesDbContext>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<PrepareGamesDatabase>>();

                    if (context.Database.GetPendingMigrations().Any())
                    {
                        try
                        {
                            context.Database.Migrate();
                        }
                        catch (SqlException ex)
                        {
                            logger.LogInformation($"Sql exception: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}