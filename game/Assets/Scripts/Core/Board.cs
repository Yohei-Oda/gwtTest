using System;
using System.Collections.Generic;
using System.Text;

namespace Othello.Core
{
    /// <summary>
    /// 8x8 の盤面。イミュータブルで、変更系の操作は常に新しい盤面を返す。
    ///
    /// 着手の適用が元の盤面を壊さないので、非合法手を弾いた側は「盤面が変化していない」
    /// ことを追加の後始末なしに保証できる。
    /// </summary>
    public sealed class Board : IEquatable<Board>
    {
        /// <summary>一辺のマス数。</summary>
        public const int Size = BoardPosition.BoardSize;

        /// <summary>盤面全体のマス数。</summary>
        public const int CellCount = Size * Size;

        private const char EmptySymbol = '.';
        private const char BlackSymbol = 'B';
        private const char WhiteSymbol = 'W';

        private static readonly Board EmptyBoard = new Board(new CellState[CellCount]);

        private readonly CellState[] cells;

        private Board(CellState[] cells)
        {
            this.cells = cells;
        }

        /// <summary>石が 1 枚も無い盤面。</summary>
        public static Board Empty
        {
            get { return EmptyBoard; }
        }

        /// <summary>
        /// 対局開始時の盤面。中央 4 マスに d5 / e4 が黒、d4 / e5 が白。
        /// </summary>
        public static Board CreateInitial()
        {
            return EmptyBoard
                .With(BoardPosition.Parse("d5"), CellState.Black)
                .With(BoardPosition.Parse("e4"), CellState.Black)
                .With(BoardPosition.Parse("d4"), CellState.White)
                .With(BoardPosition.Parse("e5"), CellState.White);
        }

        /// <summary>
        /// 8 段目から 1 段目までを上から順に並べた文字列から盤面を作る。
        /// 各行は '.'（空）/ 'B'（黒）/ 'W'（白）の 8 文字。空白は無視する。
        /// </summary>
        public static Board FromDiagram(params string[] rowsFromTop)
        {
            if (rowsFromTop == null)
            {
                throw new ArgumentNullException(nameof(rowsFromTop));
            }

            if (rowsFromTop.Length != Size)
            {
                throw new ArgumentException("盤面図は " + Size + " 行でなければならない。", nameof(rowsFromTop));
            }

            var cells = new CellState[CellCount];

            for (var line = 0; line < Size; line++)
            {
                var symbols = StripWhitespace(rowsFromTop[line]);

                if (symbols.Length != Size)
                {
                    throw new ArgumentException(
                        "盤面図の " + (line + 1) + " 行目は " + Size + " マスでなければならない。", nameof(rowsFromTop));
                }

                // 図は上が 8 段目なので、行番号を段番号へ反転させる。
                var row = Size - 1 - line;

                for (var column = 0; column < Size; column++)
                {
                    cells[new BoardPosition(column, row).Index] = ParseSymbol(symbols[column]);
                }
            }

            return new Board(cells);
        }

        /// <summary>指定マスの状態。</summary>
        public CellState this[BoardPosition position]
        {
            get { return cells[position.Index]; }
        }

        /// <summary>空きマスが 1 つも無いかどうか。</summary>
        public bool IsFull
        {
            get { return CountOf(CellState.Empty) == 0; }
        }

        /// <summary>指定マスだけを書き換えた新しい盤面を返す。</summary>
        public Board With(BoardPosition position, CellState state)
        {
            var copy = (CellState[])cells.Clone();
            copy[position.Index] = state;
            return new Board(copy);
        }

        /// <summary>指定した複数マスをまとめて書き換えた新しい盤面を返す。</summary>
        public Board WithAll(IReadOnlyList<BoardPosition> positions, CellState state)
        {
            if (positions == null)
            {
                throw new ArgumentNullException(nameof(positions));
            }

            if (positions.Count == 0)
            {
                return this;
            }

            var copy = (CellState[])cells.Clone();

            for (var i = 0; i < positions.Count; i++)
            {
                copy[positions[i].Index] = state;
            }

            return new Board(copy);
        }

        /// <summary>指定状態のマス数。</summary>
        public int CountOf(CellState state)
        {
            var count = 0;

            for (var i = 0; i < cells.Length; i++)
            {
                if (cells[i] == state)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>指定した対局者の石の数。</summary>
        public int CountOf(Player player)
        {
            return CountOf(player.ToCellState());
        }

        /// <summary>
        /// <see cref="FromDiagram"/> が受け付ける形式の盤面図を返す。先頭が 8 段目。
        /// </summary>
        public string[] ToDiagram()
        {
            var rows = new string[Size];
            var builder = new StringBuilder(Size);

            for (var line = 0; line < Size; line++)
            {
                var row = Size - 1 - line;
                builder.Length = 0;

                for (var column = 0; column < Size; column++)
                {
                    builder.Append(ToSymbol(cells[new BoardPosition(column, row).Index]));
                }

                rows[line] = builder.ToString();
            }

            return rows;
        }

        /// <inheritdoc />
        public bool Equals(Board other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other == null)
            {
                return false;
            }

            for (var i = 0; i < cells.Length; i++)
            {
                if (cells[i] != other.cells[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as Board);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = 17;

            for (var i = 0; i < cells.Length; i++)
            {
                hash = (hash * 31) + (int)cells[i];
            }

            return hash;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join("\n", ToDiagram());
        }

        private static string StripWhitespace(string row)
        {
            if (row == null)
            {
                throw new ArgumentException("盤面図の行に null は指定できない。", nameof(row));
            }

            var builder = new StringBuilder(row.Length);

            for (var i = 0; i < row.Length; i++)
            {
                if (!char.IsWhiteSpace(row[i]))
                {
                    builder.Append(row[i]);
                }
            }

            return builder.ToString();
        }

        private static CellState ParseSymbol(char symbol)
        {
            switch (char.ToUpperInvariant(symbol))
            {
                case EmptySymbol:
                case '-':
                    return CellState.Empty;
                case BlackSymbol:
                    return CellState.Black;
                case WhiteSymbol:
                    return CellState.White;
                default:
                    throw new ArgumentException("盤面図に使えない文字: " + symbol, nameof(symbol));
            }
        }

        private static char ToSymbol(CellState state)
        {
            switch (state)
            {
                case CellState.Black:
                    return BlackSymbol;
                case CellState.White:
                    return WhiteSymbol;
                default:
                    return EmptySymbol;
            }
        }
    }
}
