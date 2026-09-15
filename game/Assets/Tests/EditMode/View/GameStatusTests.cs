using NUnit.Framework;
using Othello.Core;

namespace Othello.View.Tests
{
    /// <summary>
    /// 局面から画面表示テキストを導く部分の検証（Issue #4 / AC-1〜AC-4）。
    ///
    /// <see cref="GameStatus"/> は UnityEngine に依存しないので、表示内容そのものを
    /// GameObject を作らずに固定できる。HUD 側のテストは「この文字列を出したか」だけを見る。
    /// </summary>
    public sealed class GameStatusTests
    {
        [Test]
        public void InitialState_ShowsBlackToMove()
        {
            var status = GameStatus.From(GameState.CreateInitial());

            Assert.That(status.IsGameOver, Is.False);
            Assert.That(status.PlayerToMove, Is.EqualTo(Player.Black));
            Assert.That(status.TurnText, Is.EqualTo("手番: 黒"));
        }

        [Test]
        public void TurnText_FollowsThePlayerToMove()
        {
            var status = GameStatus.From(GameState.CreateInitial().Play(BoardPosition.Parse("d3")));

            Assert.That(status.PlayerToMove, Is.EqualTo(Player.White));
            Assert.That(status.TurnText, Is.EqualTo("手番: 白"));
        }

        [Test]
        public void InitialState_ShowsTwoDiscsEach()
        {
            var status = GameStatus.From(GameState.CreateInitial());

            Assert.That(status.BlackDiscCount, Is.EqualTo(2));
            Assert.That(status.WhiteDiscCount, Is.EqualTo(2));
            Assert.That(status.BlackScoreText, Is.EqualTo("黒 2"));
            Assert.That(status.WhiteScoreText, Is.EqualTo("白 2"));
        }

        [Test]
        public void ScoreCountsTheDiscsAfterAMoveAndItsFlips()
        {
            // d3 への着手で黒石が 2 → 4（着手 1 枚 + 裏返し 1 枚）、白石が 2 → 1 になる。
            var status = GameStatus.From(GameState.CreateInitial().Play(BoardPosition.Parse("d3")));

            Assert.That(status.BlackDiscCount, Is.EqualTo(4));
            Assert.That(status.WhiteDiscCount, Is.EqualTo(1));
            Assert.That(status.BlackScoreText, Is.EqualTo("黒 4"));
            Assert.That(status.WhiteScoreText, Is.EqualTo("白 1"));
        }

        [Test]
        public void NoPassNotice_WhileBothPlayersHaveLegalMoves()
        {
            var status = GameStatus.From(GameState.CreateInitial());

            Assert.That(status.HasPassNotice, Is.False);
            Assert.That(status.PassNoticeText, Is.Empty);
        }

        [Test]
        public void PassNotice_NamesThePlayerThatWasSkipped()
        {
            // a1 黒 / b1 白 と a3 黒 / b3 白。黒が c1 に打つと白の合法手が消え、
            // 黒には c3 が残るのでパスが起きる。
            var board = Board.Empty
                .With(BoardPosition.Parse("a1"), CellState.Black)
                .With(BoardPosition.Parse("b1"), CellState.White)
                .With(BoardPosition.Parse("a3"), CellState.Black)
                .With(BoardPosition.Parse("b3"), CellState.White);

            var state = GameState.Create(board, Player.Black).Play(BoardPosition.Parse("c1"));

            Assert.That(state.PassedPlayer, Is.EqualTo(Player.White), "前提が崩れている。");

            var status = GameStatus.From(state);

            Assert.That(status.HasPassNotice, Is.True);
            Assert.That(status.PassNoticeText, Is.EqualTo("白は打てる手が無いためパスしました"));
            Assert.That(status.TurnText, Is.EqualTo("手番: 黒"), "パス後は手番が黒に戻る。");
        }

        [Test]
        public void PassNotice_ClearsOnTheNextMove()
        {
            var board = Board.Empty
                .With(BoardPosition.Parse("a1"), CellState.Black)
                .With(BoardPosition.Parse("b1"), CellState.White)
                .With(BoardPosition.Parse("a3"), CellState.Black)
                .With(BoardPosition.Parse("b3"), CellState.White);

            var passed = GameState.Create(board, Player.Black).Play(BoardPosition.Parse("c1"));
            var next = passed.Play(BoardPosition.Parse("c3"));

            Assert.That(GameStatus.From(next).HasPassNotice, Is.False);
        }

        [Test]
        public void GameOver_ShowsTheWinnerAndTheFinalScore()
        {
            // 白石が無いのでどちらも挟めない。空きは残っているが終局。
            var board = Board.Empty
                .With(BoardPosition.Parse("a1"), CellState.Black)
                .With(BoardPosition.Parse("b1"), CellState.Black)
                .With(BoardPosition.Parse("c1"), CellState.Black);

            var status = GameStatus.From(GameState.Create(board, Player.Black));

            Assert.That(status.IsGameOver, Is.True);
            Assert.That(status.ResultText, Is.EqualTo("黒の勝ち  黒 3 - 白 0"));
            Assert.That(status.BlackDiscCount, Is.EqualTo(3));
            Assert.That(status.WhiteDiscCount, Is.EqualTo(0));
        }

        [Test]
        public void GameOver_ShowsWhiteAsTheWinnerWhenItHasMoreDiscs()
        {
            var status = GameStatus.From(GameState.Create(FullBoardWith(blackRows: 3), Player.Black));

            Assert.That(status.IsGameOver, Is.True);
            Assert.That(status.ResultText, Is.EqualTo("白の勝ち  黒 24 - 白 40"));
        }

        [Test]
        public void GameOver_ShowsADrawWhenTheDiscCountsAreEqual()
        {
            var status = GameStatus.From(GameState.Create(FullBoardWith(blackRows: 4), Player.Black));

            Assert.That(status.IsGameOver, Is.True);
            Assert.That(status.ResultText, Is.EqualTo("引き分け  黒 32 - 白 32"));
        }

        [Test]
        public void GameOver_HidesTheTurnAndAnyPassNotice()
        {
            var status = GameStatus.From(GameState.Create(FullBoardWith(blackRows: 4), Player.Black));

            Assert.That(status.TurnText, Is.Empty, "終局後に手番を出してはいけない。");
            Assert.That(status.HasPassNotice, Is.False);
        }

        [Test]
        public void WhileTheGameRuns_NoResultTextIsProduced()
        {
            var status = GameStatus.From(GameState.CreateInitial());

            Assert.That(status.ResultText, Is.Empty);
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
