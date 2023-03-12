namespace TicTacToeGame.Models
{
    public class GameCell
    {
        public GameCell() { }

        public GameCell(int number)
        {
            Position = number;
        }

        public int Position { get; }
        public GameVariant Variant { get; private set; } = GameVariant.Clear;

        public int GameSessionId { get; set; }

        public bool MakeMove(GameVariant variant)
        {
            if (Variant == GameVariant.Clear)
            {
                Variant = variant;
                return true;
            }

            return false;
        }

        public void ClearCell()
        {
            Variant = GameVariant.Clear;
        }
    }
}
