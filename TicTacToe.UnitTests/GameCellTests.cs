namespace TicTacToe.UnitTests
{
    public class GameCellTests
    {
        [Fact]
        public void CreateDefaultGameCell_PositionShouldReturnZero()
        {
            // Arrange
            GameCell cell = new GameCell();

            // Assert
            Assert.Equal(0, cell.Position);
        }

        [Fact]
        public void CreateDefaultGameCell_GameVariantShouldBeClear()
        {
            // Arrange
            GameCell cell = new GameCell();

            // Assert
            Assert.Equal(GameVariant.Clear, cell.Variant);
        }

        [Fact]
        public void CreateCellWithFirstPosition_PositionShouldReturnOne()
        {
            // Arrange
            int position = 1;
            GameCell cell = new GameCell(position);

            // Assert
            Assert.Equal(1, cell.Position);
        }

        [Fact]
        public void MakeMoveInDefaultCell_GameVariantShouldChange()
        {
            // Arrange
            GameCell cell = new GameCell();
            GameVariant playVariant = GameVariant.O;

            // Act
            bool moved = cell.MakeMove(playVariant);

            // Assert
            Assert.True(moved);
            Assert.Equal(playVariant, cell.Variant);
        }

        [Fact]
        public void MakeMoveInPlayedCell_GameVariantShouldBeTheSame()
        {
            // Arrange
            GameCell cell = new GameCell();
            GameVariant playVariant = GameVariant.O;
            GameVariant differentVariant = GameVariant.X;

            // Act
            bool moved = cell.MakeMove(playVariant);
            bool movedSecondTime = cell.MakeMove(differentVariant);

            // Assert
            Assert.True(moved);
            Assert.False(movedSecondTime);
            Assert.Equal(playVariant, cell.Variant);
            Assert.NotEqual(differentVariant, cell.Variant);
        }

        [Fact]
        public void ClearPlayedCell_GameVariantShouldReturnClear()
        {
            // Arrange
            GameCell cell = new GameCell();
            GameVariant playVariant = GameVariant.X;

            // Act
            cell.MakeMove(playVariant);
            cell.ClearCell();

            // Assert
            Assert.NotEqual(playVariant, cell.Variant);
            Assert.Equal(GameVariant.Clear, cell.Variant);
        }
    }
}