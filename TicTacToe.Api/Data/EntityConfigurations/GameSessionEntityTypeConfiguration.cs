using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Data.EntityConfigurations
{
    public class GameSessionEntityTypeConfiguration : IEntityTypeConfiguration<GameSession>
    {
        public void Configure(EntityTypeBuilder<GameSession> builder)
        {
            builder.ToTable("Games").Ignore(gs => gs.Cells).HasKey(gs => gs.GameSessionId);
        }
    }
}