using System.Linq;
using NUnit.Framework;
using Othello.Core;

namespace Othello.View.Tests
{
    /// <summary>
    /// 盤面表示と着手入力をつなぐ表示モデルの検証（Issue #7 / AC-1〜AC-5）。
    /// </summary>
    public sealed class BoardPresenterTests
    {
        [Test]
        public void InitialState_PlacesTheFourCentreDiscs()
        {
            var presenter = new BoardPresenter();

            Assert.That(presenter.GetCell(BoardPosition.Parse("d5")), Is.EqualTo(CellState.Black));
            Assert.That(presenter.GetCell(BoardPosition.Parse("e4")), Is.EqualTo(CellState.Black));
            Assert.That(presenter.GetCell(BoardPosition.Parse("d4")), Is.EqualTo(CellState.White));
            Assert.That(presenter.GetCell(BoardPosition.Parse("e5")), Is.EqualTo(CellState.White));
            Assert.That(presenter.GetCell(BoardPosition.Parse("a1")), Is.EqualTo(CellState.Empty));
        }

        [Test]
        public void MoveHints_MatchTheLegalMovesOfThePlayerToMove()
        {
            var presenter = new BoardPresenter();

            var hinted = BoardPosition.All
                .Where(presenter.IsMoveHint)
                .Select(position => position.ToString())
                .OrderBy(notation => notation)
                .ToArray();

            Assert.That(presenter.State.PlayerToMove, Is.EqualTo(Player.Black));
            Assert.That(hinted, Is.EqualTo(new[] { "c4", "d3", "e6", "f5" }));
        }

        [Test]
        public void TryPlay_OnALegalCell_PlacesTheDiscAndFlipsTheFlankedOnes()
        {
            var presenter = new BoardPresenter();

            Assert.That(presenter.TryPlay(BoardPosition.Parse("d3")), Is.True);

            Assert.That(presenter.GetCell(BoardPosition.Parse("d3")), Is.EqualTo(CellState.Black));
            Assert.That(presenter.GetCell(BoardPosition.Parse("d4")), Is.EqualTo(CellState.Black));
            Assert.That(presenter.State.PlayerToMove, Is.EqualTo(Player.White));
        }

        [Test]
        public void TryPlay_OnAnIllegalCell_LeavesTheStateUntouched()
        {
            var presenter = new BoardPresenter();
            var before = presenter.State;

            Assert.That(presenter.TryPlay(BoardPosition.Parse("a1")), Is.False);

            Assert.That(presenter.State, Is.SameAs(before));
            Assert.That(presenter.GetCell(BoardPosition.Parse("a1")), Is.EqualTo(CellState.Empty));
        }

        [Test]
        public void TryPlay_RaisesChangedOnlyWhenTheMoveIsApplied()
        {
            var presenter = new BoardPresenter();
            var raised = 0;
            presenter.Changed += state => raised++;

            presenter.TryPlay(BoardPosition.Parse("a1"));
            Assert.That(raised, Is.EqualTo(0), "非合法手で表示更新が走っている。");

            presenter.TryPlay(BoardPosition.Parse("d3"));
            Assert.That(raised, Is.EqualTo(1), "着手後に表示更新が走っていない。");
        }

        [Test]
        public void MoveHints_FollowTheTurnAfterAMove()
        {
            var presenter = new BoardPresenter();
            presenter.TryPlay(BoardPosition.Parse("d3"));

            var hinted = BoardPosition.All
                .Where(presenter.IsMoveHint)
                .Select(position => position.ToString())
                .OrderBy(notation => notation)
                .ToArray();

            var expected = presenter.State.LegalMoves
                .Select(move => move.ToString())
                .OrderBy(notation => notation)
                .ToArray();

            Assert.That(presenter.State.PlayerToMove, Is.EqualTo(Player.White));
            Assert.That(hinted, Is.Not.Empty);
            Assert.That(hinted, Is.EqualTo(expected));
        }

        [Test]
        public void OccupiedCellsAreNeverHinted()
        {
            var presenter = new BoardPresenter();

            foreach (var position in BoardPosition.All)
            {
                if (presenter.GetCell(position) == CellState.Empty)
                {
                    continue;
                }

                Assert.That(presenter.IsMoveHint(position), Is.False, position + " は石があるのにヒント表示されている。");
            }
        }
    }
}
