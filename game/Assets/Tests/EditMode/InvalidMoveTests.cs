using NUnit.Framework;

namespace Othello.Core.Tests
{
    /// <summary>
    /// 非合法手を適用したときの挙動の検証（Issue #2 / AC-5）。
    /// </summary>
    public sealed class InvalidMoveTests
    {
        [Test]
        public void TryPlay_ReturnsFalseForAnIllegalMove()
        {
            var state = GameState.CreateInitial();

            Assert.That(state.TryPlay(BoardPosition.Parse("a1"), out var next), Is.False);
            Assert.That(next, Is.Null);
        }

        [Test]
        public void TryPlay_ReturnsFalseForAnOccupiedCell()
        {
            var state = GameState.CreateInitial();

            Assert.That(state.TryPlay(BoardPosition.Parse("d4"), out _), Is.False);
        }

        [Test]
        public void TryPlay_LeavesTheBoardUnchangedForAnIllegalMove()
        {
            var state = GameState.CreateInitial();
            var before = state.Board;

            state.TryPlay(BoardPosition.Parse("a1"), out _);

            Assert.That(state.Board, Is.EqualTo(before));
            Assert.That(state.Board, Is.EqualTo(Board.CreateInitial()));
            Assert.That(state.PlayerToMove, Is.EqualTo(Player.Black));
        }

        [Test]
        public void Play_ThrowsInvalidMoveExceptionForAnIllegalMove()
        {
            var state = GameState.CreateInitial();

            var exception = Assert.Throws<InvalidMoveException>(
                () => state.Play(BoardPosition.Parse("a1")));

            Assert.That(exception.Position, Is.EqualTo(BoardPosition.Parse("a1")));
            Assert.That(exception.Player, Is.EqualTo(Player.Black));
        }

        [Test]
        public void Play_LeavesTheBoardUnchangedWhenItThrows()
        {
            var state = GameState.CreateInitial();

            Assert.Throws<InvalidMoveException>(() => state.Play(BoardPosition.Parse("a1")));

            Assert.That(state.Board, Is.EqualTo(Board.CreateInitial()));
        }

        [Test]
        public void TryPlay_ReturnsFalseOnceTheGameIsOver()
        {
            var board = Board.Empty
                .With(BoardPosition.Parse("a1"), CellState.Black)
                .With(BoardPosition.Parse("b1"), CellState.Black);

            var state = GameState.Create(board, Player.Black);

            Assert.That(state.IsGameOver, Is.True);
            Assert.That(state.TryPlay(BoardPosition.Parse("c1"), out _), Is.False);
        }
    }
}
