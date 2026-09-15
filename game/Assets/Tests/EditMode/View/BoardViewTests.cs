using System.Linq;
using NUnit.Framework;
using Othello.Core;
using UnityEngine;

namespace Othello.View.Tests
{
    /// <summary>
    /// uGUI の盤面表示とクリック入力の検証（Issue #7 / AC-1〜AC-5）。
    ///
    /// クリックは <c>Button.onClick</c> を直接発火させて再現する。EventSystem の
    /// ヒットテストは Unity 側の責務なので、ここでは押された結果だけを検証する。
    /// </summary>
    public sealed class BoardViewTests
    {
        private GameObject root;
        private BoardView view;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("BoardViewTestRoot", typeof(RectTransform));
            view = root.AddComponent<BoardView>();
            view.EnsureInitialized();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
        }

        [Test]
        public void EnsureInitialized_BuildsOneCellPerSquare()
        {
            Assert.That(view.Cells.Count, Is.EqualTo(BoardPosition.BoardSize * BoardPosition.BoardSize));

            foreach (var position in BoardPosition.All)
            {
                Assert.That(view.GetCell(position), Is.Not.Null, position + " のセルが作られていない。");
            }
        }

        [Test]
        public void EnsureInitialized_IsIdempotent()
        {
            view.EnsureInitialized();

            Assert.That(view.Cells.Count, Is.EqualTo(BoardPosition.BoardSize * BoardPosition.BoardSize));
        }

        [Test]
        public void InitialBoard_RendersTheFourCentreDiscsInTheRightColours()
        {
            AssertDisc("d5", view.BlackDiscColor);
            AssertDisc("e4", view.BlackDiscColor);
            AssertDisc("d4", view.WhiteDiscColor);
            AssertDisc("e5", view.WhiteDiscColor);

            var occupied = BoardPosition.All.Where(position => view.GetCell(position).IsDiscVisible)
                .Select(position => position.ToString())
                .OrderBy(notation => notation)
                .ToArray();

            Assert.That(occupied, Is.EqualTo(new[] { "d4", "d5", "e4", "e5" }));
        }

        [Test]
        public void ClickingALegalCell_PlacesADiscAndRepaintsTheFlippedOnes()
        {
            Click("d3");

            AssertDisc("d3", view.BlackDiscColor);
            AssertDisc("d4", view.BlackDiscColor);
            AssertDisc("d5", view.BlackDiscColor);
            AssertDisc("e5", view.WhiteDiscColor);
        }

        [Test]
        public void ClickingAnIllegalCell_LeavesTheBoardUnchanged()
        {
            var before = view.Presenter.State;

            Click("a1");

            Assert.That(view.Presenter.State, Is.SameAs(before));
            Assert.That(view.GetCell(BoardPosition.Parse("a1")).IsDiscVisible, Is.False);
        }

        [Test]
        public void ClickingAnOccupiedCell_LeavesTheBoardUnchanged()
        {
            var before = view.Presenter.State;

            Click("d4");

            Assert.That(view.Presenter.State, Is.SameAs(before));
            AssertDisc("d4", view.WhiteDiscColor);
        }

        [Test]
        public void LegalMovesForThePlayerToMoveAreHighlighted()
        {
            var highlighted = HighlightedCells();

            Assert.That(highlighted, Is.EqualTo(new[] { "c4", "d3", "e6", "f5" }));
        }

        [Test]
        public void HighlightsFollowTheTurnAfterAMove()
        {
            Click("d3");

            var expected = view.Presenter.State.LegalMoves
                .Select(move => move.ToString())
                .OrderBy(notation => notation)
                .ToArray();

            Assert.That(view.Presenter.State.PlayerToMove, Is.EqualTo(Player.White));
            Assert.That(HighlightedCells(), Is.EqualTo(expected));
        }

        private string[] HighlightedCells()
        {
            return BoardPosition.All
                .Where(position => view.GetCell(position).IsHintVisible)
                .Select(position => position.ToString())
                .OrderBy(notation => notation)
                .ToArray();
        }

        private void Click(string notation)
        {
            view.GetCell(BoardPosition.Parse(notation)).Button.onClick.Invoke();
        }

        private void AssertDisc(string notation, Color expected)
        {
            var cell = view.GetCell(BoardPosition.Parse(notation));

            Assert.That(cell.IsDiscVisible, Is.True, notation + " に石が表示されていない。");
            Assert.That(cell.DiscColor, Is.EqualTo(expected), notation + " の石の色が違う。");
        }
    }
}
