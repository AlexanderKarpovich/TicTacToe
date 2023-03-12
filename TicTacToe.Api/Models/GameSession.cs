namespace TicTacToe.Api.Models
{
    public class GameSession
    {
        public GameSession() { }

        public int GameSessionId { get; set; }

        public Player? Player1 { get; private set; }
        public Player? Player2 { get; private set; }
        public Player? PlayerTurn { get; private set; }
        public Player? Winner { get; private set; }

        public bool IsEmpty { get; private set; }

        public DateTime CreationTime { get; } = DateTime.Now;

        public IReadOnlyList<GameCell>? Cells { get; set; }

        public bool JoinGame(Player player)
        {
            if (Player1 is not null && Player2 is not null)
            {
                return false;
            }

            if (Player1 is null)
            {
                Player1 = player;
                player.Variant = Random.Shared.Next(1000) % 2 == 0 ? GameVariant.X : GameVariant.O;

                IsEmpty = false;
            }
            else
            {
                Player2 = player;
                Player2.Variant = Player1.Variant == GameVariant.X ? GameVariant.O : GameVariant.X;
                StartGame();
            }

            return true;
        }

        public void LeaveGame(Player player)
        {
            if (Player1?.Name == player.Name)
            {
                if (Player2 is not null)
                {
                    Player1 = Player2;
                    Player2 = null;
                }
                else
                {
                    Player1 = null;
                    IsEmpty = true;
                }
                ResetGame();
                return;
            }

            if (Player2?.Name == player.Name)
            {
                Player2 = null;
                ResetGame();
                return;
            }

            throw new ArgumentException("The given player is not in the game", nameof(player));
        }

        public bool MakeMove(string playerName, int position)
        {
            if (playerName != PlayerTurn?.Name)
            {
                throw new ArgumentException("The given player is not the one who should make move", nameof(playerName));
            }

            bool isSucceeded = Cells![position].MakeMove(PlayerTurn.Variant);

            if (isSucceeded)
            {
                PlayerTurn = PlayerTurn != Player1 ? Player1 : Player2;
                CheckWin();
            }

            return isSucceeded;
        }

        public static List<GameCell> GenerateGameField()
        {
            var list = new List<GameCell>();
            foreach (int number in Enumerable.Range(0, 9))
            {
                list.Add(new GameCell(number));
            }
            return list;
        }

        private void CheckWin()
        {
            CheckRows();
            CheckColumns();
            CheckDiagonals();
        }

        private void CheckDiagonals()
        {
            // Primary diagonal
            if (Cells![0].Variant == Cells[4].Variant && 
                Cells[4].Variant == Cells[8].Variant && 
                Cells[0].Variant != GameVariant.Clear)
            {
                Winner = Player1?.Variant == Cells[0].Variant ? Player1 : Player2;
            }
            // Secondary diagonal
            else if (Cells[2].Variant == Cells[4].Variant && 
                Cells[4].Variant == Cells[6].Variant && 
                Cells[2].Variant != GameVariant.Clear)
            {
                Winner = Player1?.Variant == Cells[2].Variant ? Player1 : Player2;
            }
        }

        private void CheckColumns()
        {
            // Column check
            for (int i = 0; i < 3; i++)
            {
                if (Cells![i].Variant == Cells[i + 3].Variant && 
                    Cells[i + 3].Variant == Cells[i + 6].Variant && 
                    Cells[i].Variant != GameVariant.Clear)
                {
                    Winner = Player1?.Variant == Cells[i].Variant ? Player1 : Player2;
                }
            }
        }

        private void CheckRows()
        {
            // Row check
            for (int i = 0; i < 8; i += 3)
            {
                if (Cells![i].Variant == Cells[i + 1].Variant && 
                    Cells[i + 1].Variant == Cells[i + 2].Variant &&
                    Cells[i].Variant != GameVariant.Clear)
                {
                    Winner = Player1?.Variant == Cells[i].Variant ? Player1 : Player2;
                }
            }
        }

        private void ResetGame()
        {
            foreach (GameCell cell in Cells!)
            {
                cell.ClearCell();
            }

            PlayerTurn = null;
            Winner = null;
        }

        private void StartGame()
        {
            PlayerTurn = Random.Shared.Next(1000) % 2 == 0 ? Player1 : Player2;
        }
    }
}
