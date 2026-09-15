using NUnit.Framework;
using Othello.Core;

namespace Othello.View.Tests
{
    /// <summary>
    /// 対局のやり直しと終局後の入力停止の検証（Issue #4 / AC-5, AC-6）。
    /// </summary>
    public sealed class BoardPresenterRestartTests
    {
        [Test]
        public void Restart_ReturnsTheBoardAndTheTurnToTheOpeningPosition()
        {
            var presenter = new BoardPresenter();
            presenter.TryPlay(BoardPosition.Parse("d3"));

            presenter.Restart();

            Assert.That(presenter.State.PlayerToMove, Is.EqualTo(Player.Black));
            Assert.That(presenter.State.IsGameOver, Is.False);
            Assert.That(presenter.State.Board, Is.EqualTo(Board.CreateInitial()));
            Assert.That(presenter.GetCell(BoardPosition.Parse("d3")), Is.EqualTo(CellState.Empty));
        }

        [Test]
        public void Restart_RaisesChangedSoTheDisplayRepaints()
        {
            var presenter = new BoardPresenter();
            presenter.TryPlay(BoardPosition.Parse("d3"));

            var raised = 0;
            presenter.Changed += state => raised++;

            presenter.Restart();

            Assert.That(raised, Is.EqualTo(1));
        }

        [Test]
        public void Restart_RestoresTheOpeningHints()
        {
            var presenter = new BoardPresenter();
            presenter.TryPlay(BoardPosition.Parse("d3"));

            presenter.Restart();

            Assert.That(presenter.IsMoveHint(BoardPosition.Parse("d3")), Is.True);
            Assert.That(presenter.IsMoveHint(BoardPosition.Parse("c3")), Is.False);
        }

        [Test]
        public void Restart_WorksFromAFinishedGame()
        {
            var presenter = new BoardPresenter(FinishedGame());

            Assert.That(presenter.State.IsGameOver, Is.True, "前提が崩れている。");

            presenter.Restart();

            Assert.That(presenter.State.IsGameOver, Is.False);
            Assert.That(presenter.TryPlay(BoardPosition.Parse("d3")), Is.True, "やり直した対局で着手できない。");
        }

        [Test]
        public void AfterTheGameEnds_NoCellAcceptsAMove()
        {
            var presenter = new BoardPresenter(FinishedGame());
            var before = presenter.State;

            foreach (var position in BoardPosition.All)
            {
                Assert.That(presenter.TryPlay(position), Is.False, position + " が終局後に着手を受け付けている。");
            }

            Assert.That(presenter.State, Is.SameAs(before));
        }

        [Test]
        public void AfterTheGameEnds_NoCellIsHinted()
        {
            var presenter = new BoardPresenter(FinishedGame());

            foreach (var position in BoardPosition.All)
            {
                Assert.That(presenter.IsMoveHint(position), Is.False, position + " が終局後にヒント表示されている。");
            }
        }

        /// <summary>白石が 1 枚も無く、どちらも挟めない終局局面。空きマスは残っている。</summary>
        private static GameState FinishedGame()
        {
            var board = Board.Empty
                .With(BoardPosition.Parse("a1"), CellState.Black)
                .With(BoardPosition.Parse("b1"), CellState.Black)
                .With(BoardPosition.Parse("c1"), CellState.Black);

            return GameState.Create(board, Player.Black);
        }
    }
}
