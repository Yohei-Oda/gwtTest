using NUnit.Framework;

namespace Othello.Core.Tests
{
    /// <summary>
    /// 盤面表現と初期配置の検証（Issue #2 / AC-2）。
    /// </summary>
    public sealed class BoardTests
    {
        [Test]
        public void Board_IsEightByEight()
        {
            Assert.That(Board.Size, Is.EqualTo(8));
            Assert.That(Board.CellCount, Is.EqualTo(64));
        }

        [Test]
        public void CreateInitial_PlacesBlackOnD5AndE4()
        {
            var board = Board.CreateInitial();

            Assert.That(board[BoardPosition.Parse("d5")], Is.EqualTo(CellState.Black));
            Assert.That(board[BoardPosition.Parse("e4")], Is.EqualTo(CellState.Black));
        }

        [Test]
        public void CreateInitial_PlacesWhiteOnD4AndE5()
        {
            var board = Board.CreateInitial();

            Assert.That(board[BoardPosition.Parse("d4")], Is.EqualTo(CellState.White));
            Assert.That(board[BoardPosition.Parse("e5")], Is.EqualTo(CellState.White));
        }

        [Test]
        public void CreateInitial_HasExactlyTwoDiscsPerPlayerAndNothingElse()
        {
            var board = Board.CreateInitial();

            Assert.That(board.CountOf(Player.Black), Is.EqualTo(2));
            Assert.That(board.CountOf(Player.White), Is.EqualTo(2));
            Assert.That(board.CountOf(CellState.Empty), Is.EqualTo(60));
            Assert.That(board.IsFull, Is.False);
        }

        [Test]
        public void CreateInitial_LeavesEveryCellOutsideTheCentreFourEmpty()
        {
            var board = Board.CreateInitial();

            foreach (var position in BoardPosition.All)
            {
                var isCentre =
                    position == BoardPosition.Parse("d4") ||
                    position == BoardPosition.Parse("d5") ||
                    position == BoardPosition.Parse("e4") ||
                    position == BoardPosition.Parse("e5");

                if (isCentre)
                {
                    continue;
                }

                Assert.That(board[position], Is.EqualTo(CellState.Empty), position + " は空でなければならない。");
            }
        }

        [Test]
        public void FromDiagram_RoundTripsThroughToDiagram()
        {
            var board = Board.CreateInitial();

            Assert.That(Board.FromDiagram(board.ToDiagram()), Is.EqualTo(board));
        }

        [Test]
        public void IsFull_IsTrueWhenNoCellIsEmpty()
        {
            var board = Board.FromDiagram(
                "BBBBBBBB",
                "BBBBBBBB",
                "BBBBBBBB",
                "BBBBBBBB",
                "WWWWWWWW",
                "WWWWWWWW",
                "WWWWWWWW",
                "WWWWWWWW");

            Assert.That(board.IsFull, Is.True);
            Assert.That(board.CountOf(Player.Black), Is.EqualTo(32));
            Assert.That(board.CountOf(Player.White), Is.EqualTo(32));
        }
    }
}
