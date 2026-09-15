using NUnit.Framework;

namespace Othello.Core.Tests
{
    /// <summary>
    /// 手番の移動とパスの検証（Issue #2 / AC-6）。
    /// </summary>
    public sealed class TurnAndPassTests
    {
        [Test]
        public void Play_PassesTheTurnToTheOpponent()
        {
            var state = GameState.CreateInitial();

            var next = state.Play(BoardPosition.Parse("d3"));

            Assert.That(next.PlayerToMove, Is.EqualTo(Player.White));
            Assert.That(next.PassedPlayer, Is.Null);
            Assert.That(next.IsGameOver, Is.False);
        }

        [Test]
        public void Play_KeepsAlternatingOverSeveralMoves()
        {
            var state = GameState.CreateInitial();

            state = state.Play(BoardPosition.Parse("d3"));
            Assert.That(state.PlayerToMove, Is.EqualTo(Player.White));

            state = state.Play(state.LegalMoves[0]);
            Assert.That(state.PlayerToMove, Is.EqualTo(Player.Black));

            state = state.Play(state.LegalMoves[0]);
            Assert.That(state.PlayerToMove, Is.EqualTo(Player.White));
        }

        [Test]
        public void Play_ReturnsTheTurnToTheMoverWhenTheOpponentHasNoLegalMove()
        {
            // a1 黒 / b1 白 の列に加え、a3 黒 / b3 白 の列を用意する。
            // 黒が c1 に着手すると白の合法手は無くなるが、黒には c3 が残る。
            var board = Board.Empty
                .With(BoardPosition.Parse("a1"), CellState.Black)
                .With(BoardPosition.Parse("b1"), CellState.White)
                .With(BoardPosition.Parse("a3"), CellState.Black)
                .With(BoardPosition.Parse("b3"), CellState.White);

            var state = GameState.Create(board, Player.Black);
            Assert.That(state.PlayerToMove, Is.EqualTo(Player.Black));

            var next = state.Play(BoardPosition.Parse("c1"));

            Assert.That(next.IsGameOver, Is.False, "黒には c3 が残っているので終局ではない。");
            Assert.That(next.PassedPlayer, Is.EqualTo(Player.White), "白は合法手が無いのでパスとなる。");
            Assert.That(next.PlayerToMove, Is.EqualTo(Player.Black), "パス後は手番が黒に戻る。");
            Assert.That(next.LegalMoves, Is.EqualTo(new[] { BoardPosition.Parse("c3") }));
        }

        [Test]
        public void Create_SkipsAPlayerThatHasNoLegalMoveFromTheStart()
        {
            var board = Board.Empty
                .With(BoardPosition.Parse("a1"), CellState.Black)
                .With(BoardPosition.Parse("b1"), CellState.White);

            // 白には合法手が無いので、白手番で作っても黒に渡る。
            var state = GameState.Create(board, Player.White);

            Assert.That(state.IsGameOver, Is.False);
            Assert.That(state.PassedPlayer, Is.EqualTo(Player.White));
            Assert.That(state.PlayerToMove, Is.EqualTo(Player.Black));
        }

        [Test]
        public void CreateInitial_StartsWithBlackToMove()
        {
            var state = GameState.CreateInitial();

            Assert.That(state.PlayerToMove, Is.EqualTo(Player.Black));
            Assert.That(state.PassedPlayer, Is.Null);
            Assert.That(state.IsGameOver, Is.False);
        }

        [Test]
        public void Opponent_IsTheOtherPlayer()
        {
            Assert.That(Player.Black.Opponent(), Is.EqualTo(Player.White));
            Assert.That(Player.White.Opponent(), Is.EqualTo(Player.Black));
        }
    }
}
