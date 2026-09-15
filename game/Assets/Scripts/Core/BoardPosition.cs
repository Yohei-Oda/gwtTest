using System;
using System.Collections.Generic;

namespace Othello.Core
{
    /// <summary>
    /// 盤面上の 1 マスを指す座標。
    ///
    /// <see cref="Column"/> は棋譜の筋 a〜h に 0〜7 で対応し、
    /// <see cref="Row"/> は段 1〜8 に 0〜7 で対応する。
    /// 盤外の座標は生成できないので、この型を受け取る側は範囲検査を省略できる。
    /// </summary>
    public readonly struct BoardPosition : IEquatable<BoardPosition>
    {
        /// <summary>一辺のマス数。</summary>
        public const int BoardSize = 8;

        private const char FirstFile = 'a';
        private const char FirstRank = '1';

        /// <summary>筋。0 が a、7 が h。</summary>
        public int Column { get; }

        /// <summary>段。0 が 1 段目、7 が 8 段目。</summary>
        public int Row { get; }

        /// <summary>盤内の座標を生成する。盤外なら例外を投げる。</summary>
        public BoardPosition(int column, int row)
        {
            if (column < 0 || column >= BoardSize)
            {
                throw new ArgumentOutOfRangeException(nameof(column), column, "筋は 0〜7 でなければならない。");
            }

            if (row < 0 || row >= BoardSize)
            {
                throw new ArgumentOutOfRangeException(nameof(row), row, "段は 0〜7 でなければならない。");
            }

            Column = column;
            Row = row;
        }

        /// <summary>盤面の全マスを a1, b1, ... h8 の順に列挙する。</summary>
        public static IEnumerable<BoardPosition> All
        {
            get
            {
                for (var row = 0; row < BoardSize; row++)
                {
                    for (var column = 0; column < BoardSize; column++)
                    {
                        yield return new BoardPosition(column, row);
                    }
                }
            }
        }

        /// <summary>与えられた座標が盤内かどうか。</summary>
        public static bool IsOnBoard(int column, int row)
        {
            return column >= 0 && column < BoardSize && row >= 0 && row < BoardSize;
        }

        /// <summary>盤内なら座標を生成する。盤外なら false を返す。</summary>
        public static bool TryCreate(int column, int row, out BoardPosition position)
        {
            if (!IsOnBoard(column, row))
            {
                position = default;
                return false;
            }

            position = new BoardPosition(column, row);
            return true;
        }

        /// <summary>"d5" のような棋譜表記を座標に変換する。</summary>
        public static BoardPosition Parse(string notation)
        {
            if (!TryParse(notation, out var position))
            {
                throw new FormatException("盤面座標として解釈できない表記: " + (notation ?? "<null>"));
            }

            return position;
        }

        /// <summary>"d5" のような棋譜表記を座標に変換する。解釈できなければ false を返す。</summary>
        public static bool TryParse(string notation, out BoardPosition position)
        {
            position = default;

            if (notation == null || notation.Length != 2)
            {
                return false;
            }

            var file = char.ToLowerInvariant(notation[0]);
            var rank = notation[1];

            return TryCreate(file - FirstFile, rank - FirstRank, out position);
        }

        /// <summary>この座標から <paramref name="direction"/> へ 1 マス進んだ座標。盤外なら false。</summary>
        public bool TryOffset(BoardDirection direction, out BoardPosition neighbour)
        {
            return TryCreate(Column + direction.DeltaColumn, Row + direction.DeltaRow, out neighbour);
        }

        /// <summary>盤面配列上の連番。a1 が 0、h8 が 63。</summary>
        internal int Index
        {
            get { return (Row * BoardSize) + Column; }
        }

        /// <inheritdoc />
        public bool Equals(BoardPosition other)
        {
            return Column == other.Column && Row == other.Row;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is BoardPosition other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Index;
        }

        /// <summary>棋譜表記（例: "d5"）を返す。</summary>
        public override string ToString()
        {
            return new string(new[] { (char)(FirstFile + Column), (char)(FirstRank + Row) });
        }

        public static bool operator ==(BoardPosition left, BoardPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BoardPosition left, BoardPosition right)
        {
            return !left.Equals(right);
        }
    }
}
