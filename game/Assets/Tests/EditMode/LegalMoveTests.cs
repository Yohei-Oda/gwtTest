using System.Linq;
using NUnit.Framework;

namespace Othello.Core.Tests
{
    /// <summary>
    /// 合法手列挙の検証（Issue #2 / AC-3）。
    /// </summary>
    public sealed class LegalMoveTests
    {
        [Test]
        public void InitialPosition_BlackHasExactlyFourLegalMoves()
        {
            var state = GameState.CreateInitial();

            Assert.That(state.PlayerToMove, Is.EqualTo(Player.Black));
            Assert.That(state.LegalMoves.Count, Is.EqualTo(4));
        }

        [Test]
        public void InitialPosition_BlackLegalMovesAreD3C4F5E6()
        {
            var state = GameState.CreateInitial();

            var actual = state.LegalMoves.Select(move => move.ToString()).OrderBy(move => move).ToArray();

            Assert.That(actual, Is.EqualTo(new[] { "c4", "d3", "e6", "f5" }));
        }

        [Test]
        public void InitialPosition_WhiteAlsoHasExactlyFourLegalMoves()
        {
            var moves = OthelloRules.FindLegalMoves(Board.CreateInitial(), Player.White);

            var actual = moves.Select(move => move.ToString()).OrderBy(move => move).ToArray();

            Assert.That(actual, Is.EqualTo(new[] { "c5", "d6", "e3", "f4" }));
        }

        [Test]
        public void IsLegalMove_AgreesWithTheEnumeratedLegalMoves()
        {
            var state = GameState.CreateInitial();

            foreach (var position in BoardPosition.All)
            {
                var expected = state.LegalMoves.Contains(position);

                Assert.That(state.IsLegalMove(position), Is.EqualTo(expected), position + " の判定が一覧と一致しない。");
            }
        }

        [Test]
        public void OccupiedCellIsNeverALegalMove()
        {
            var state = GameState.CreateInitial();

            Assert.That(state.IsLegalMove(BoardPosition.Parse("d4")), Is.False);
            Assert.That(state.IsLegalMove(BoardPosition.Parse("d5")), Is.False);
        }

        [Test]
        public void EmptyCellWithoutAnyFlankedDiscIsNotALegalMove()
        {
            var state = GameState.CreateInitial();

            Assert.That(state.IsLegalMove(BoardPosition.Parse("a1")), Is.False);
            Assert.That(state.IsLegalMove(BoardPosition.Parse("d6")), Is.False);
        }
    }
}
