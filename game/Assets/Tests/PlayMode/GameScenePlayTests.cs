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
    /// Game.unity を実際に再生して盤面表示と着手入力を通しで確かめる（Issue #7 / AC-1〜AC-5, AC-8）。
    ///
    /// クリックは EventSystem の pointerClick として流し込むので、シーンに
    /// EventSystem と GraphicRaycaster が正しく置かれていることも同時に検証される。
    /// 環境変数 <c>OTHELLO_SCREENSHOT_DIR</c> を指定して実行すると、初期配置と
    /// 着手後の画面を PNG で書き出す。
    /// </summary>
    public sealed class GameScenePlayTests
    {
        private const string ScreenshotDirectoryVariable = "OTHELLO_SCREENSHOT_DIR";

        [UnityTest]
        public IEnumerator GameScene_ShowsTheBoardAndPlaysAClickedMove()
        {
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            yield return null;

            var view = UnityEngine.Object.FindAnyObjectByType<BoardView>();

            Assert.That(view, Is.Not.Null, "Game.unity に BoardView が置かれていない。");
            Assert.That(EventSystem.current, Is.Not.Null, "Game.unity に EventSystem が置かれていない。");

            // AC-1: 8x8 の盤面と、初期配置の 4 石が正しい位置・色で並ぶ。
            Assert.That(view.Cells.Count, Is.EqualTo(BoardPosition.BoardSize * BoardPosition.BoardSize));
            AssertDisc(view, "d5", view.BlackDiscColor);
            AssertDisc(view, "e4", view.BlackDiscColor);
            AssertDisc(view, "d4", view.WhiteDiscColor);
            AssertDisc(view, "e5", view.WhiteDiscColor);

            // AC-5: 手番（黒）の合法手がヒント表示されている。
            Assert.That(view.Presenter.State.PlayerToMove, Is.EqualTo(Player.Black));
            Assert.That(view.GetCell(BoardPosition.Parse("d3")).IsHintVisible, Is.True, "合法手 d3 が提示されていない。");
            Assert.That(view.GetCell(BoardPosition.Parse("a1")).IsHintVisible, Is.False, "非合法手 a1 が提示されている。");

            yield return Capture("01-initial");

            // AC-3: 非合法手をクリックしても局面は動かない。
            var before = view.Presenter.State;

            Click(view, "a1");
            yield return null;

            Assert.That(view.Presenter.State, Is.SameAs(before), "非合法手のクリックで局面が変化した。");
            Assert.That(view.GetCell(BoardPosition.Parse("a1")).IsDiscVisible, Is.False);

            // AC-2 / AC-4: 合法手をクリックすると着手され、挟まれた d4 の白石が黒へ裏返る。
            Click(view, "d3");
            yield return null;

            AssertDisc(view, "d3", view.BlackDiscColor);
            AssertDisc(view, "d4", view.BlackDiscColor);
            Assert.That(view.Presenter.State.PlayerToMove, Is.EqualTo(Player.White));

            // AC-5: ヒントは次の手番（白）のものへ切り替わる。
            Assert.That(view.GetCell(BoardPosition.Parse("d3")).IsHintVisible, Is.False);
            Assert.That(view.GetCell(BoardPosition.Parse("c3")).IsHintVisible, Is.True, "白番の合法手 c3 が提示されていない。");

            yield return Capture("02-after-d3");
        }

        private static void Click(BoardView view, string notation)
        {
            var cell = view.GetCell(BoardPosition.Parse(notation));

            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(null, cell.transform.position),
            };

            ExecuteEvents.Execute(cell.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }

        private static void AssertDisc(BoardView view, string notation, Color expected)
        {
            var cell = view.GetCell(BoardPosition.Parse(notation));

            Assert.That(cell.IsDiscVisible, Is.True, notation + " に石が表示されていない。");
            Assert.That(cell.DiscColor, Is.EqualTo(expected), notation + " の石の色が違う。");
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
