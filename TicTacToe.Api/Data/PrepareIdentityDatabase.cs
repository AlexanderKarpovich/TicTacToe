using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Data
{
    public class PrepareIdentityDatabase
    {
        private const string adminUser = "Admin";
        private const string adminPassword = "Secret123$";

        public static async Task EnsurePopulated(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            AppIdentityDbContext context = app.ApplicationServices.CreateScope()
                .ServiceProvider.GetRequiredService<AppIdentityDbContext>();

            if (environment.IsProduction() && context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }

            UserManager<GameUser> userManager = app.ApplicationServices.CreateScope()
                .ServiceProvider.GetRequiredService<UserManager<GameUser>>();

            GameUser? user = await userManager.FindByIdAsync(adminUser);
            if (user == null)
            {
                user = new GameUser("Admin")
                {
                    Email = "admin@example.com",
                    PhoneNumber = "555-1234"
                };

                await userManager.CreateAsync(user, adminPassword);
            }
        }
    }
}
