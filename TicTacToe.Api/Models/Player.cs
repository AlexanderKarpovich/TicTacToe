namespace TicTacToe.Api.Models
{
    public class Player
    {
        public int PlayerId { get; set; }
        public string? Name { get; set; }
        public GameVariant Variant { get; set; }
    }
}
