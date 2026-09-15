using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using Othello.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Othello.View.PlayTests
{
    /// <summary>
    /// Game.unity を再生し、1 局を終局まで通して表示とリスタートを確かめる（Issue #4 / AC-1〜AC-6, AC-8）。
    ///
    /// クリックは EventSystem の pointerClick として流すので、実際に遊んだときと同じ経路を通る。
    /// 環境変数 <c>OTHELLO_SCREENSHOT_DIR</c> を指定すると、対局開始・パス発生・終局・
    /// リスタート直後の画面を PNG で書き出す。
    /// </summary>
    public sealed class GameFlowPlayTests
    {
        private const string ScreenshotDirectoryVariable = "OTHELLO_SCREENSHOT_DIR";

        [UnityTest]
        public IEnumerator GameScene_PlaysOneGameThroughToTheVerdictAndRestarts()
        {
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            yield return null;

            var view = UnityEngine.Object.FindAnyObjectByType<BoardView>();
            var hud = UnityEngine.Object.FindAnyObjectByType<GameHudView>();

            Assert.That(view, Is.Not.Null, "Game.unity に BoardView が置かれていない。");
            Assert.That(hud, Is.Not.Null, "Game.unity に GameHudView が置かれていない。");
            Assert.That(EventSystem.current, Is.Not.Null, "Game.unity に EventSystem が置かれていない。");

            // AC-1 / AC-2: 対局開始時点で手番とスコアが出ている。
            Assert.That(hud.TurnText, Is.EqualTo("手番: 黒"));
            Assert.That(hud.BlackScoreText, Is.EqualTo("黒 2"));
            Assert.That(hud.WhiteScoreText, Is.EqualTo("白 2"));
            Assert.That(hud.IsResultVisible, Is.False);

            yield return Capture("01-opening");

            // 合法手の先頭を選び続けて終局まで進める。
            var passSeen = false;
            var passShotTaken = false;

            for (var move = 0; move < Board.CellCount && !view.Presenter.State.IsGameOver; move++)
            {
                var playerToMove = view.Presenter.State.PlayerToMove;

                Click(view, view.Presenter.State.LegalMoves[0]);
                yield return null;

                var state = view.Presenter.State;

                // AC-1: 手番表示は終局まで実際の手番と一致し続ける。
                if (!state.IsGameOver)
                {
                    Assert.That(
                        hud.TurnText,
                        Is.EqualTo(state.PlayerToMove == Player.Black ? "手番: 黒" : "手番: 白"),
                        "手番表示が局面とずれた。");
                }

                // AC-2: スコア表示は裏返りを含めて盤面の石数と一致し続ける。
                Assert.That(hud.BlackScoreText, Is.EqualTo("黒 " + state.Board.CountOf(Player.Black)));
                Assert.That(hud.WhiteScoreText, Is.EqualTo("白 " + state.Board.CountOf(Player.White)));

                // AC-3: パスが起きた手では、飛ばされた側を名指しする通知が出ている。
                if (state.PassedPlayer != null)
                {
                    passSeen = true;

                    Assert.That(hud.IsPassNoticeVisible, Is.True, "パスが起きたのに通知が出ていない。");
                    Assert.That(
                        hud.PassNoticeText,
                        Does.Contain(state.PassedPlayer == Player.Black ? "黒" : "白"),
                        "パス通知が飛ばされた側を示していない。");
                    Assert.That(state.PlayerToMove, Is.EqualTo(playerToMove), "パス後に手番が戻っていない。");

                    if (!passShotTaken)
                    {
                        passShotTaken = true;
                        yield return Capture("02-pass");
                    }
                }
                else
                {
                    Assert.That(hud.IsPassNoticeVisible, Is.False, "パスが無い手でパス通知が残っている。");
                }
            }

            Assert.That(view.Presenter.State.IsGameOver, Is.True, "対局が終局まで進まなかった。");

            // AC-4: 勝敗と最終スコアが画面に出る。手番表示は消える。
            var result = view.Presenter.State.GetResult();
            var verdict = result.IsDraw ? "引き分け" : (result.Winner == Player.Black ? "黒の勝ち" : "白の勝ち");

            Assert.That(hud.IsResultVisible, Is.True, "終局したのに勝敗が表示されていない。");
            Assert.That(
                hud.ResultText,
                Is.EqualTo(verdict + "  黒 " + result.BlackDiscCount + " - 白 " + result.WhiteDiscCount));
            Assert.That(hud.TurnText, Is.Empty, "終局後も手番が表示されている。");
            Assert.That(hud.IsPassNoticeVisible, Is.False);

            Debug.Log(
                "[Issue #4] 通しプレイ結果: " + hud.ResultText +
                " / パス発生: " + (passSeen ? "あり" : "なし"));

            yield return Capture("03-game-over");

            // AC-6: 終局後は盤面のどこをクリックしても局面が動かない。
            var finished = view.Presenter.State;

            foreach (var position in BoardPosition.All)
            {
                Click(view, position);
            }

            yield return null;

            Assert.That(view.Presenter.State, Is.SameAs(finished), "終局後のクリックで局面が動いた。");
            Assert.That(hud.IsResultVisible, Is.True);

            // AC-5: リスタートで盤面・手番・スコアが初期状態へ戻り、続けて打てる。
            ClickButton(hud.RestartButton.gameObject);
            yield return null;

            Assert.That(view.Presenter.State.IsGameOver, Is.False);
            Assert.That(view.Presenter.State.Board, Is.EqualTo(Board.CreateInitial()));
            Assert.That(hud.TurnText, Is.EqualTo("手番: 黒"));
            Assert.That(hud.BlackScoreText, Is.EqualTo("黒 2"));
            Assert.That(hud.WhiteScoreText, Is.EqualTo("白 2"));
            Assert.That(hud.IsResultVisible, Is.False);
            Assert.That(view.GetCell(BoardPosition.Parse("d3")).IsHintVisible, Is.True);

            yield return Capture("04-after-restart");

            Click(view, BoardPosition.Parse("d3"));
            yield return null;

            Assert.That(view.GetCell(BoardPosition.Parse("d3")).IsDiscVisible, Is.True, "リスタート後に着手できない。");
            Assert.That(hud.TurnText, Is.EqualTo("手番: 白"));
        }

        private static void Click(BoardView view, BoardPosition position)
        {
            ClickButton(view.GetCell(position).gameObject);
        }

        private static void ClickButton(GameObject target)
        {
            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(null, target.transform.position),
            };

            ExecuteEvents.Execute(target, pointer, ExecuteEvents.pointerClickHandler);
        }

        private static IEnumerator Capture(string name)
        {
            var directory = Environment.GetEnvironmentVariable(ScreenshotDirectoryVariable);

            if (string.IsNullOrEmpty(directory))
            {
                yield break;
            }

            Directory.CreateDirectory(directory);

            var path = Path.Combine(directory, name + ".png");

            // シーンを読み込んだ直後は Game View がまだ一度も描かれておらず、
            // 撮っても背景色しか写らない。数フレーム進めてから撮る。
            for (var frame = 0; frame < 3; frame++)
            {
                yield return new WaitForEndOfFrame();
            }

            // CaptureScreenshotAsTexture はエディタの Game View 描画の途中を掴むので
            // 中身が壊れる。ファイル書き出し版に任せ、出てくるまで待つ。
            ScreenCapture.CaptureScreenshot(path);

            for (var frame = 0; frame < 120 && !File.Exists(path); frame++)
            {
                yield return null;
            }
        }
    }
}
