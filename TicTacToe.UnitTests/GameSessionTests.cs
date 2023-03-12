using System;

namespace TicTacToe.UnitTests
{
    public class GameSessionTests
    {
        private GameSession game;

        public GameSessionTests()
        {
            game = new GameSession();
            game.Cells = GameSession.GenerateGameField();
        }

        [Fact]
        public void JoinGameAsFirstPlayer_GameSessionStateShouldBeCorrect()
        {
            // Arrange
            var player = new Player() { Name = "New player" };

            // Act
            bool joined = game.JoinGame(player);

            // Assert
            Assert.True(joined);
            Assert.False(game.IsEmpty);

            Assert.NotNull(game.Player1);
            Assert.Equal(player, game.Player1);

            Assert.Null(game.Player2);
            Assert.Null(game.PlayerTurn);
            Assert.Null(game.Winner);
        }

        [Fact]
        public void JoinGameAsSecondPlayer_GameSessionStateShouldBeCorrect()
        {
            // Arrange
            var player1 = new Player() { Name = "Player1" };
            var player2 = new Player() { Name = "Player2" };
            game.JoinGame(player1);

            // Act
            bool secondJoined = game.JoinGame(player2);

            // Assert
            Assert.NotNull(game.Player1);
            Assert.NotNull(game.Player2);
            Assert.NotNull(game.PlayerTurn);

            Assert.Equal(player1, game.Player1);
            Assert.Equal(player2, game.Player2);

            Assert.NotEqual(game.Player1!.Variant, game.Player2!.Variant);

            Assert.True(secondJoined);
            Assert.True(game.PlayerTurn == player1 || game.PlayerTurn == player2);
            Assert.False(game.IsEmpty);
            
            Assert.Null(game.Winner);
        }

        [Fact]
        public void TryJoinGameAsThirdPlayer_MethodShouldReturnFalse()
        {
            // Arrange
            var player1 = new Player() { Name = "Player1" };
            var player2 = new Player() { Name = "Player2" };
            var randomPlayer = new Player() { Name = "Random" };
            game.JoinGame(player1);
            game.JoinGame(player2);

            // Act
            bool joined = game.JoinGame(randomPlayer);

            // Assert
            Assert.False(joined);
        }

        [Fact]
        public void LeaveGameAsFirstPlayer_WithoutSecondPlayer_GameSessionStateShouldBeCorrect()
        {
            // Arrange
            var player1 = new Player() { Name = "Player1" };
            game.JoinGame(player1);

            // Act
            game.LeaveGame(player1);

            // Assert
            Assert.True(game.IsEmpty);
            
            Assert.Null(game.Player1);
            Assert.Null(game.Player2);
            Assert.Null(game.PlayerTurn);
            Assert.Null(game.Winner);
        }

        [Fact]
        public void LeaveGameAsFirstPlayer_WithSecondPlayer_GameSessionStateShouldBeCorrect()
        {
            // Arrange
            var player1 = new Player() { Name = "Player1" };
            var player2 = new Player() { Name = "Player2" };
            game.JoinGame(player1);
            game.JoinGame(player2);

            // Act
            game.LeaveGame(player1);

            // Assert
            Assert.NotNull(game.Player1);
            Assert.Equal(player2, game.Player1);

            Assert.False(game.IsEmpty);
            
            Assert.Null(game.Player2);
            Assert.Null(game.PlayerTurn);
            Assert.Null(game.Winner);
        }

        [Fact]
        public void LeaveGameAsSecondPlayer_GameSessionStateShouldBeCorrect()
        {
            // Arrange
            var player1 = new Player() { Name = "Player1" };
            var player2 = new Player() { Name = "Player2" };
            game.JoinGame(player1);
            game.JoinGame(player2);

            // Act
            game.LeaveGame(player2);

            // Assert
            Assert.NotNull(game.Player1);
            Assert.Equal(player1, game.Player1);

            Assert.False(game.IsEmpty);
            
            Assert.Null(game.Player2);
            Assert.Null(game.PlayerTurn);
            Assert.Null(game.Winner);
        }

        [Fact]
        public void LeaveGameAsFirstPlayer_AfterMakingMove_CellsStateShouldBeReset()
        {
            // Arrange
            var player1 = new Player() { Name = "Player1" };
            var player2 = new Player() { Name = "Player2" };
            game.JoinGame(player1);
            game.JoinGame(player2);

            // Act
            var playerTurn = game.PlayerTurn;
            game.MakeMove(playerTurn?.Name!, 4);
            game.LeaveGame(player1);

            // Assert
            Assert.All(game.Cells, cell => cell.Variant.Equals(GameVariant.Clear));
        }

        [Fact]
        public void LeaveGameAsSecondPlayer_AfterMakingMove_CellsStateShouldBeReset()
        {
            // Arrange
            var player1 = new Player() { Name = "Player1" };
            var player2 = new Player() { Name = "Player2" };
            game.JoinGame(player1);
            game.JoinGame(player2);

            // Act
            var playerTurn = game.PlayerTurn;
            game.MakeMove(playerTurn?.Name!, 4);
            game.LeaveGame(player2);

            // Assert
            Assert.All(game.Cells, cell => cell.Variant.Equals(GameVariant.Clear));
        }

        [Fact]
        public void LeaveGameAsWrongPlayer_ShouldThrowArgumentException()
        {
            // Arrange
            var player1 = new Player() { Name = "Player1" };
            var player2 = new Player() { Name = "Player2" };
            var randomPlayer = new Player() { Name = "Random" };
            game.JoinGame(player1);
            game.JoinGame(player2);

            Action leave = () => game.LeaveGame(randomPlayer);
            Type exceptionType = typeof(ArgumentException);

            // Act & Assert
            Assert.Throws(exceptionType, leave);
        }

        [Fact]
        public void MakeMoveAsCorrectPlayer_UnplayedCell_ShouldMakeMove()
        {
            // Arrange
            var player1 = new Player() { Name = "Player1" };
            var player2 = new Player() { Name = "Player2" };
            game.JoinGame(player1);
            game.JoinGame(player2);

            // Act
            var playerTurn = game.PlayerTurn;
            bool moved = game.MakeMove(playerTurn?.Name!, 4);

            // Assert
            Assert.True(moved);
        }

        [Fact]
        public void MakeMoveAsCorrectPlayer_PlayedCell_ShouldNotMakeMove()
        {
            // Arrange
            var player1 = new Player() { Name = "Player1" };
            var player2 = new Player() { Name = "Player2" };
            game.JoinGame(player1);
            game.JoinGame(player2);

            var playerTurn = game.PlayerTurn;
            game.MakeMove(playerTurn?.Name!, 4);

            // Act
            playerTurn = game.PlayerTurn;
            bool moved = game.MakeMove(playerTurn?.Name!, 4);

            // Assert
            Assert.False(moved);
        }

        [Fact]
        public void MakeMoveAsWrongPlayer_ShouldThrowArgumentException()
        {
            // Arrange
            var player1 = new Player() { Name = "Player1" };
            var player2 = new Player() { Name = "Player2" };
            var randomPlayer = new Player() { Name = "Random" };
            game.JoinGame(player1);
            game.JoinGame(player2);

            Action move = () => game.MakeMove(randomPlayer.Name, 0);
            Type exceptionType = typeof(ArgumentException);

            // Act & Assert
            Assert.Throws(exceptionType, move);
        }

        [Fact]
        public void WinGameAsFirstPlayer_WinnerShouldReturnFirstPlayer()
        {
            // Arrange
            var player1 = new Player() { Name = "Player1" };
            var player2 = new Player() { Name = "Player2" };
            game.JoinGame(player1);
            game.JoinGame(player2);

            var playerTurn = game.PlayerTurn;
            var firstPlayerMoves = new int[] { 0, 1, 2 };
            var secondPlayerMoves = new int[] { 3, 5, 7};

            // Act
            for (int i = 0; i < 6; i++)
            {
                if (playerTurn == player1)
                {
                    game.MakeMove(playerTurn?.Name!, firstPlayerMoves[i / 2]);
                }
                else
                {
                    game.MakeMove(playerTurn?.Name!, secondPlayerMoves[i / 2]);
                }
                playerTurn = game.PlayerTurn;
            }

            // Assert
            Assert.NotNull(game.Winner);
            Assert.Equal(player1, game.Winner);
        }

        [Fact]
        public void WinGameAsSecondPlayer_WinnerShouldReturnSecondPlayer()
        {
            // Arrange
            var player1 = new Player() { Name = "Player1" };
            var player2 = new Player() { Name = "Player2" };
            game.JoinGame(player1);
            game.JoinGame(player2);

            var playerTurn = game.PlayerTurn;
            var firstPlayerMoves = new int[] { 3, 5, 7};
            var secondPlayerMoves = new int[] { 0, 1, 2 };

            // Act
            for (int i = 0; i < 6; i++)
            {
                if (playerTurn == player1)
                {
                    game.MakeMove(playerTurn?.Name!, firstPlayerMoves[i / 2]);
                }
                else
                {
                    game.MakeMove(playerTurn?.Name!, secondPlayerMoves[i / 2]);
                }
                playerTurn = game.PlayerTurn;
            }

            // Assert
            Assert.NotNull(game.Winner);
            Assert.Equal(player2, game.Winner);
        }

        [Fact]
        public void GenerateGameField_ShouldReturnNineElementsFromZeroToEight()
        {
            // Act
            var cells = GameSession.GenerateGameField();

            // Assert
            Assert.Equal(9, cells.Count);

            for (int i = 0; i < 9; i++)
            {
                Assert.Equal(i, cells[i].Position);
            }

            Assert.All(cells, cell => cell.Variant.Equals(GameVariant.Clear));
        }
    }
}