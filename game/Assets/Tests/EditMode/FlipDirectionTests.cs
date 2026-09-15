using NUnit.Framework;

namespace Othello.Core.Tests
{
    /// <summary>
    /// 8 方向すべての裏返しを 1 方向ずつ個別に検証する（Issue #2 / AC-4）。
    ///
    /// いずれのケースも d4 に黒を置く。d4 の隣に白を 1 枚、その先に黒のアンカーを
    /// 1 枚だけ置いた盤面を使うので、対象方向以外では裏返しが起きない。
    /// 着手後に黒 3 / 白 0 になれば、その方向だけが正しく裏返ったことになる。
    /// </summary>
    public sealed class FlipDirectionTests
    {
        private const string Move = "d4";

        [Test]
        public void Play_FlipsToTheNorth()
        {
            AssertSingleDiscFlipped(flipped: "d5", anchor: "d6");
        }

        [Test]
        public void Play_FlipsToTheSouth()
        {
            AssertSingleDiscFlipped(flipped: "d3", anchor: "d2");
        }

        [Test]
        public void Play_FlipsToTheEast()
        {
            AssertSingleDiscFlipped(flipped: "e4", anchor: "f4");
        }

        [Test]
        public void Play_FlipsToTheWest()
        {
            AssertSingleDiscFlipped(flipped: "c4", anchor: "b4");
        }

        [Test]
        public void Play_FlipsToTheNorthEast()
        {
            AssertSingleDiscFlipped(flipped: "e5", anchor: "f6");
        }

        [Test]
        public void Play_FlipsToTheNorthWest()
        {
            AssertSingleDiscFlipped(flipped: "c5", anchor: "b6");
        }

        [Test]
        public void Play_FlipsToTheSouthEast()
        {
            AssertSingleDiscFlipped(flipped: "e3", anchor: "f2");
        }

        [Test]
        public void Play_FlipsToTheSouthWest()
        {
            AssertSingleDiscFlipped(flipped: "c3", anchor: "b2");
        }

        [Test]
        public void Play_FlipsEveryDiscOfALongerRun()
        {
            var board = Board.Empty
                .With(BoardPosition.Parse("e4"), CellState.White)
                .With(BoardPosition.Parse("f4"), CellState.White)
                .With(BoardPosition.Parse("g4"), CellState.White)
                .With(BoardPosition.Parse("h4"), CellState.Black);

            var next = GameState.Create(board, Player.Black).Play(BoardPosition.Parse(Move));

            Assert.That(next.Board.CountOf(Player.White), Is.EqualTo(0));
            Assert.That(next.Board.CountOf(Player.Black), Is.EqualTo(5));
        }

        [Test]
        public void Play_FlipsInAllEligibleDirectionsAtOnce()
        {
            var board = Board.Empty
                .With(BoardPosition.Parse("d5"), CellState.White)
                .With(BoardPosition.Parse("d6"), CellState.Black)
                .With(BoardPosition.Parse("e4"), CellState.White)
                .With(BoardPosition.Parse("f4"), CellState.Black);

            var next = GameState.Create(board, Player.Black).Play(BoardPosition.Parse(Move));

            Assert.That(next.Board[BoardPosition.Parse("d5")], Is.EqualTo(CellState.Black));
            Assert.That(next.Board[BoardPosition.Parse("e4")], Is.EqualTo(CellState.Black));
            Assert.That(next.Board.CountOf(Player.White), Is.EqualTo(0));
            Assert.That(next.Board.CountOf(Player.Black), Is.EqualTo(5));
        }

        [Test]
        public void Play_DoesNotFlipARunThatIsNotTerminatedByOwnDisc()
        {
            // d4 の東に白が 2 枚続くが、その先は空なので挟めていない。
            var board = Board.Empty
                .With(BoardPosition.Parse("e4"), CellState.White)
                .With(BoardPosition.Parse("f4"), CellState.White);

            Assert.That(OthelloRules.FindFlips(board, Player.Black, BoardPosition.Parse(Move)), Is.Empty);
        }

        [Test]
        public void Play_DoesNotFlipAcrossAnEmptyGap()
        {
            // d4 の東は 空(e4) を挟むので、f4 の白と g4 の黒には届かない。
            var board = Board.Empty
                .With(BoardPosition.Parse("f4"), CellState.White)
                .With(BoardPosition.Parse("g4"), CellState.Black);

            Assert.That(OthelloRules.FindFlips(board, Player.Black, BoardPosition.Parse(Move)), Is.Empty);
        }

        /// <summary>
        /// d4 に黒を着手したとき <paramref name="flipped"/> の白だけが裏返り、
        /// <paramref name="anchor"/> の黒が挟み込みの終端として働くことを検証する。
        /// </summary>
        private static void AssertSingleDiscFlipped(string flipped, string anchor)
        {
            var move = BoardPosition.Parse(Move);
            var flippedPosition = BoardPosition.Parse(flipped);
            var anchorPosition = BoardPosition.Parse(anchor);

            var board = Board.Empty
                .With(flippedPosition, CellState.White)
                .With(anchorPosition, CellState.Black);

            var state = GameState.Create(board, Player.Black);

            Assert.That(state.IsLegalMove(move), Is.True, Move + " は " + flipped + " を挟む合法手のはず。");
            Assert.That(OthelloRules.FindFlips(board, Player.Black, move), Is.EqualTo(new[] { flippedPosition }));

            var next = state.Play(move);

            Assert.That(next.Board[move], Is.EqualTo(CellState.Black), "着手点に黒が置かれていない。");
            Assert.That(next.Board[flippedPosition], Is.EqualTo(CellState.Black), flipped + " が裏返っていない。");
            Assert.That(next.Board[anchorPosition], Is.EqualTo(CellState.Black), "終端の黒が変化してはならない。");
            Assert.That(next.Board.CountOf(Player.White), Is.EqualTo(0), "他方向まで裏返っている。");
            Assert.That(next.Board.CountOf(Player.Black), Is.EqualTo(3), "他方向まで裏返っている。");
        }
    }
}
