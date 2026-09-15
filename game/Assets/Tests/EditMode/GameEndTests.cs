using System;
using NUnit.Framework;

namespace Othello.Core.Tests
{
    /// <summary>
    /// 終局判定と勝敗判定の検証（Issue #2 / AC-7）。
    /// </summary>
    public sealed class GameEndTests
    {
        [Test]
        public void GameIsOver_WhenNeitherPlayerHasALegalMove()
        {
            // 白石が 1 枚も無いので、どちらも挟めない。盤面には空きが残っている。
            var board = Board.Empty
                .With(BoardPosition.Parse("a1"), CellState.Black)
                .With(BoardPosition.Parse("b1"), CellState.Black)
                .With(BoardPosition.Parse("c1"), CellState.Black);

            var state = GameState.Create(board, Player.Black);

            Assert.That(state.Board.IsFull, Is.False);
            Assert.That(state.IsGameOver, Is.True);
            Assert.That(state.LegalMoves, Is.Empty);
        }

        [Test]
        public void GameIsOver_WhenTheBoardIsFull()
        {
            var state = GameState.Create(FullBoardWith(blackRows: 4), Player.Black);

            Assert.That(state.Board.IsFull, Is.True);
            Assert.That(state.IsGameOver, Is.True);
        }

        [Test]
        public void Winner_IsThePlayerWithMoreDiscs_Black()
        {
            var board = Board.Empty
                .With(BoardPosition.Parse("a1"), CellState.Black)
                .With(BoardPosition.Parse("b1"), CellState.Black)
                .With(BoardPosition.Parse("c1"), CellState.Black);

            var result = GameState.Create(board, Player.Black).GetResult();

            Assert.That(result.BlackDiscCount, Is.EqualTo(3));
            Assert.That(result.WhiteDiscCount, Is.EqualTo(0));
            Assert.That(result.Winner, Is.EqualTo(Player.Black));
            Assert.That(result.IsDraw, Is.False);
        }

        [Test]
        public void Winner_IsThePlayerWithMoreDiscs_White()
        {
            var result = GameState.Create(FullBoardWith(blackRows: 3), Player.Black).GetResult();

            Assert.That(result.BlackDiscCount, Is.EqualTo(24));
            Assert.That(result.WhiteDiscCount, Is.EqualTo(40));
            Assert.That(result.Winner, Is.EqualTo(Player.White));
            Assert.That(result.IsDraw, Is.False);
        }

        [Test]
        public void Result_IsADrawWhenBothPlayersHaveTheSameDiscCount()
        {
            var result = GameState.Create(FullBoardWith(blackRows: 4), Player.Black).GetResult();

            Assert.That(result.BlackDiscCount, Is.EqualTo(32));
            Assert.That(result.WhiteDiscCount, Is.EqualTo(32));
            Assert.That(result.IsDraw, Is.True);
            Assert.That(result.Winner, Is.Null);
        }

        [Test]
        public void GetResult_ThrowsWhileTheGameIsStillRunning()
        {
            var state = GameState.CreateInitial();

            Assert.That(state.IsGameOver, Is.False);
            Assert.Throws<InvalidOperationException>(() => state.GetResult());
        }

        /// <summary>上から <paramref name="blackRows"/> 行を黒、残りを白で埋めた満杯の盤面。</summary>
        private static Board FullBoardWith(int blackRows)
        {
            var rows = new string[Board.Size];
            for (var i = 0; i < rows.Length; i++)
            {
                rows[i] = new string(i < blackRows ? 'B' : 'W', Board.Size);
            }

            return Board.FromDiagram(rows);
        }
    }
}
