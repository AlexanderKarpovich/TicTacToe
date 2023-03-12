using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Data
{
    public class AppIdentityDbContext : IdentityDbContext<GameUser>
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options) { }
    }
}
