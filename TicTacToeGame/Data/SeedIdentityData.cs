using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicTacToeGame.Models;

namespace TicTacToeGame.Data
{
    public class SeedIdentityData
    {
        private const string adminUser = "Admin";
        private const string adminPassword = "Secret123$";

        public static async Task EnsurePopulated(IApplicationBuilder app)
        {
            AppIdentityDbContext context = app.ApplicationServices.CreateScope()
                .ServiceProvider.GetRequiredService<AppIdentityDbContext>();

            if (context.Database.GetPendingMigrations().Any())
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
