using NUnit.Framework;
using Othello.Core;
using UnityEngine;

namespace Othello.View.Tests
{
    /// <summary>
    /// 手番・スコア・パス・勝敗の画面表示とリスタート操作の検証（Issue #4 / AC-1〜AC-6）。
    ///
    /// 表示テキストの中身は <see cref="GameStatusTests"/> が固定しているので、ここでは
    /// 「盤面の動きが表示に届いているか」と「ボタンが対局を作り直すか」を見る。
    /// </summary>
    public sealed class GameHudViewTests
    {
        private GameObject root;
        private BoardView board;
        private GameHudView hud;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("GameHudTestRoot", typeof(RectTransform));

            var boardObject = new GameObject("Board", typeof(RectTransform));
            boardObject.transform.SetParent(root.transform, false);
            board = boardObject.AddComponent<BoardView>();

            var hudObject = new GameObject("Hud", typeof(RectTransform));
            hudObject.transform.SetParent(root.transform, false);
            hud = hudObject.AddComponent<GameHudView>();

            hud.Bind(board);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
        }

        [Test]
        public void Bind_ShowsTheOpeningPositionWithoutWaitingForAMove()
        {
            Assert.That(hud.TurnText, Is.EqualTo("手番: 黒"));
            Assert.That(hud.BlackScoreText, Is.EqualTo("黒 2"));
            Assert.That(hud.WhiteScoreText, Is.EqualTo("白 2"));
            Assert.That(hud.IsPassNoticeVisible, Is.False);
            Assert.That(hud.IsResultVisible, Is.False);
        }

        [Test]
        public void TheTurnDisplayFollowsEveryMove()
        {
            Click("d3");
            Assert.That(hud.TurnText, Is.EqualTo("手番: 白"));

            Click(board.Presenter.State.LegalMoves[0]);
            Assert.That(hud.TurnText, Is.EqualTo("手番: 黒"));
        }

        [Test]
        public void TheScoreDisplayCountsTheFlippedDiscs()
        {
            Click("d3");

            Assert.That(hud.BlackScoreText, Is.EqualTo("黒 4"));
            Assert.That(hud.WhiteScoreText, Is.EqualTo("白 1"));
        }

        [Test]
        public void AnIllegalClickLeavesTheDisplayUntouched()
        {
            Click("a1");

            Assert.That(hud.TurnText, Is.EqualTo("手番: 黒"));
            Assert.That(hud.BlackScoreText, Is.EqualTo("黒 2"));
            Assert.That(hud.WhiteScoreText, Is.EqualTo("白 2"));
        }

        [Test]
        public void PassNotice_AppearsWhenTheSideToMoveIsSkipped()
        {
            var passed = GameState.Create(PassBoard(), Player.Black).Play(BoardPosition.Parse("c1"));

            Assert.That(passed.PassedPlayer, Is.EqualTo(Player.White), "前提が崩れている。");

            hud.Show(passed);

            Assert.That(hud.IsPassNoticeVisible, Is.True);
            Assert.That(hud.PassNoticeText, Is.EqualTo("白は打てる手が無いためパスしました"));
        }

        [Test]
        public void PassNotice_DisappearsOnTheFollowingMove()
        {
            var passed = GameState.Create(PassBoard(), Player.Black).Play(BoardPosition.Parse("c1"));
            hud.Show(passed);

            hud.Show(passed.Play(BoardPosition.Parse("c3")));

            Assert.That(hud.IsPassNoticeVisible, Is.False);
        }

        [Test]
        public void GameOver_ShowsTheVerdictAndHidesTheTurn()
        {
            hud.Show(FinishedGame());

            Assert.That(hud.IsResultVisible, Is.True);
            Assert.That(hud.ResultText, Is.EqualTo("黒の勝ち  黒 3 - 白 0"));
            Assert.That(hud.TurnText, Is.Empty);
        }

        [Test]
        public void PlayingARealGameToTheEndShowsAVerdictThatMatchesTheScore()
        {
            PlayToTheEnd();

            var result = board.Presenter.State.GetResult();

            Assert.That(hud.IsResultVisible, Is.True, "終局したのに勝敗が表示されていない。");
            Assert.That(hud.BlackScoreText, Is.EqualTo("黒 " + result.BlackDiscCount));
            Assert.That(hud.WhiteScoreText, Is.EqualTo("白 " + result.WhiteDiscCount));
            Assert.That(hud.ResultText, Does.Contain(result.BlackDiscCount + " - 白 " + result.WhiteDiscCount));
        }

        [Test]
        public void AfterTheGameEnds_ClickingTheBoardChangesNothing()
        {
            PlayToTheEnd();

            var before = board.Presenter.State;
            var resultText = hud.ResultText;

            foreach (var position in BoardPosition.All)
            {
                Click(position);
            }

            Assert.That(board.Presenter.State, Is.SameAs(before), "終局後のクリックで局面が動いた。");
            Assert.That(hud.ResultText, Is.EqualTo(resultText));
        }

        [Test]
        public void RestartButton_ReturnsTheBoardAndTheDisplayToTheOpeningPosition()
        {
            PlayToTheEnd();

            hud.RestartButton.onClick.Invoke();

            Assert.That(board.Presenter.State.IsGameOver, Is.False);
            Assert.That(board.Presenter.State.Board, Is.EqualTo(Board.CreateInitial()));
            Assert.That(hud.TurnText, Is.EqualTo("手番: 黒"));
            Assert.That(hud.BlackScoreText, Is.EqualTo("黒 2"));
            Assert.That(hud.WhiteScoreText, Is.EqualTo("白 2"));
            Assert.That(hud.IsResultVisible, Is.False);
            Assert.That(hud.IsPassNoticeVisible, Is.False);
        }

        [Test]
        public void RestartButton_ClearsTheStonesFromTheBoardDisplay()
        {
            Click("d3");

            hud.RestartButton.onClick.Invoke();

            Assert.That(board.GetCell(BoardPosition.Parse("d3")).IsDiscVisible, Is.False);
            Assert.That(board.GetCell(BoardPosition.Parse("d4")).DiscColor, Is.EqualTo(board.WhiteDiscColor));
            Assert.That(board.GetCell(BoardPosition.Parse("d3")).IsHintVisible, Is.True);
        }

        [Test]
        public void AfterRestart_TheBoardAcceptsMovesAgain()
        {
            PlayToTheEnd();
            hud.RestartButton.onClick.Invoke();

            Click("d3");

            Assert.That(board.GetCell(BoardPosition.Parse("d3")).IsDiscVisible, Is.True);
            Assert.That(hud.TurnText, Is.EqualTo("手番: 白"));
        }

        /// <summary>合法手の先頭を選び続けて対局を終局まで進める。</summary>
        private void PlayToTheEnd()
        {
            for (var move = 0; move < Board.CellCount && !board.Presenter.State.IsGameOver; move++)
            {
                Click(board.Presenter.State.LegalMoves[0]);
            }

            Assert.That(board.Presenter.State.IsGameOver, Is.True, "対局が終局まで進まなかった。");
        }

        private void Click(string notation)
        {
            Click(BoardPosition.Parse(notation));
        }

        private void Click(BoardPosition position)
        {
            board.GetCell(position).Button.onClick.Invoke();
        }

        /// <summary>黒が c1 に打つと白の合法手が消え、黒には c3 が残る盤面。</summary>
        private static Board PassBoard()
        {
            return Board.Empty
                .With(BoardPosition.Parse("a1"), CellState.Black)
                .With(BoardPosition.Parse("b1"), CellState.White)
                .With(BoardPosition.Parse("a3"), CellState.Black)
                .With(BoardPosition.Parse("b3"), CellState.White);
        }

        /// <summary>白石が 1 枚も無く、どちらも挟めない終局局面。</summary>
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
