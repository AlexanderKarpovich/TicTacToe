using Microsoft.EntityFrameworkCore;
using TicTacToeGame.Data.EntityConfigurations;
using TicTacToeGame.Models;

namespace TicTacToeGame.Data
{
    public class GamesDbContext : DbContext
    {
        public GamesDbContext(DbContextOptions<GamesDbContext> options) : base(options) { }

        public DbSet<GameSession> GameSessions { get; set; } = default!;
        public DbSet<Player> Players { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new GameSessionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new GameCellEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PlayerEntityTypeConfiguration());
        }
    }
}
