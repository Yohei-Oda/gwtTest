using System;
using System.Collections.Generic;

namespace Othello.Core
{
    /// <summary>
    /// 盤面上の 8 方向のいずれか。挟み込みの探索はこの 8 方向に対して行う。
    ///
    /// 北は段が増える向き（1 段目 → 8 段目）、東は筋が増える向き（a → h）。
    /// </summary>
    public readonly struct BoardDirection : IEquatable<BoardDirection>
    {
        /// <summary>北（段が増える向き）。</summary>
        public static readonly BoardDirection North = new BoardDirection(0, 1, "North");

        /// <summary>北東。</summary>
        public static readonly BoardDirection NorthEast = new BoardDirection(1, 1, "NorthEast");

        /// <summary>東（筋が増える向き）。</summary>
        public static readonly BoardDirection East = new BoardDirection(1, 0, "East");

        /// <summary>南東。</summary>
        public static readonly BoardDirection SouthEast = new BoardDirection(1, -1, "SouthEast");

        /// <summary>南。</summary>
        public static readonly BoardDirection South = new BoardDirection(0, -1, "South");

        /// <summary>南西。</summary>
        public static readonly BoardDirection SouthWest = new BoardDirection(-1, -1, "SouthWest");

        /// <summary>西。</summary>
        public static readonly BoardDirection West = new BoardDirection(-1, 0, "West");

        /// <summary>北西。</summary>
        public static readonly BoardDirection NorthWest = new BoardDirection(-1, 1, "NorthWest");

        private static readonly BoardDirection[] AllDirections =
        {
            North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest,
        };

        private readonly string name;

        private BoardDirection(int deltaColumn, int deltaRow, string name)
        {
            DeltaColumn = deltaColumn;
            DeltaRow = deltaRow;
            this.name = name;
        }

        /// <summary>1 歩進むときの筋の増分。</summary>
        public int DeltaColumn { get; }

        /// <summary>1 歩進むときの段の増分。</summary>
        public int DeltaRow { get; }

        /// <summary>8 方向すべて。</summary>
        public static IReadOnlyList<BoardDirection> All
        {
            get { return AllDirections; }
        }

        /// <inheritdoc />
        public bool Equals(BoardDirection other)
        {
            return DeltaColumn == other.DeltaColumn && DeltaRow == other.DeltaRow;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is BoardDirection other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return ((DeltaColumn + 1) * 3) + (DeltaRow + 1);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return name ?? "Unknown";
        }
    }
}
