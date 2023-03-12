using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Data.EntityConfigurations
{
    public class GameCellEntityTypeConfiguration : IEntityTypeConfiguration<GameCell>
    {
        public void Configure(EntityTypeBuilder<GameCell> builder)
        {
            builder.ToTable("GameCells").HasKey(gc => new { gc.GameSessionId, gc.Position });
            builder.HasOne<GameSession>().WithMany(gs => gs.Cells);
        }
    }
}