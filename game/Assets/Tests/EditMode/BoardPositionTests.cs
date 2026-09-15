using System;
using System.Linq;
using NUnit.Framework;

namespace Othello.Core.Tests
{
    /// <summary>
    /// 盤面座標の表記と境界の検証（Issue #2 / AC-1 の基礎型）。
    /// </summary>
    public sealed class BoardPositionTests
    {
        [Test]
        public void Parse_MapsFileLetterToColumnAndRankDigitToRow()
        {
            var d5 = BoardPosition.Parse("d5");

            Assert.That(d5.Column, Is.EqualTo(3));
            Assert.That(d5.Row, Is.EqualTo(4));
        }

        [Test]
        public void ToString_ReturnsAlgebraicNotation()
        {
            Assert.That(new BoardPosition(0, 0).ToString(), Is.EqualTo("a1"));
            Assert.That(new BoardPosition(7, 7).ToString(), Is.EqualTo("h8"));
        }

        [Test]
        public void Constructor_RejectsCoordinatesOutsideTheBoard()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BoardPosition(8, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BoardPosition(0, -1));
        }

        [Test]
        public void TryParse_FailsForMalformedNotation()
        {
            Assert.That(BoardPosition.TryParse("i1", out _), Is.False);
            Assert.That(BoardPosition.TryParse("a9", out _), Is.False);
            Assert.That(BoardPosition.TryParse("d", out _), Is.False);
            Assert.That(BoardPosition.TryParse(null, out _), Is.False);
        }

        [Test]
        public void All_EnumeratesEveryCellExactlyOnce()
        {
            var all = BoardPosition.All.ToArray();

            Assert.That(all.Length, Is.EqualTo(64));
            Assert.That(all.Distinct().Count(), Is.EqualTo(64));
        }
    }
}
