using TicTacToe.Api.Models;

namespace TicTacToe.Api.Dtos
{
    public class GameSessionReadDto
    {
        public int GameSessionId { get; set; }
        public Player? Player1 { get; set; }
        public Player? Player2 { get; set; }
        public Player? PlayerTurn { get; set; }
        public Player? Winner { get; set; }
        public IEnumerable<GameCell> Cells { get; set; } = default!;
    }
}
