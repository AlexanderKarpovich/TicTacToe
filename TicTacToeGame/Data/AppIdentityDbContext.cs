using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TicTacToeGame.Models;

namespace TicTacToeGame.Data
{
    public class AppIdentityDbContext : IdentityDbContext<GameUser>
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options) { }
    }
}
