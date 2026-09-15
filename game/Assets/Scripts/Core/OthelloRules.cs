using System;
using System.Collections.Generic;

namespace Othello.Core
{
    /// <summary>
    /// 標準オセロ規則の判定。盤面と手番から合法手と裏返る石を導く。
    /// 状態を持たないので、任意の局面に対して単体で使える。
    /// </summary>
    public static class OthelloRules
    {
        private static readonly BoardPosition[] NoFlips = new BoardPosition[0];

        /// <summary>
        /// <paramref name="player"/> が <paramref name="move"/> に着手したとき裏返る石をすべて返す。
        /// 着手が非合法（マスが空でない、または 1 枚も挟めない）なら空を返す。
        /// </summary>
        public static IReadOnlyList<BoardPosition> FindFlips(Board board, Player player, BoardPosition move)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (board[move] != CellState.Empty)
            {
                return NoFlips;
            }

            var own = player.ToCellState();
            var opponent = player.Opponent().ToCellState();
            List<BoardPosition> flips = null;

            var directions = BoardDirection.All;

            for (var i = 0; i < directions.Count; i++)
            {
                CollectFlipsInDirection(board, move, directions[i], own, opponent, ref flips);
            }

            return flips ?? (IReadOnlyList<BoardPosition>)NoFlips;
        }

        /// <summary>
        /// <paramref name="player"/> が着手できるマスをすべて返す。1 手も無ければ空を返す。
        /// </summary>
        public static IReadOnlyList<BoardPosition> FindLegalMoves(Board board, Player player)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            var moves = new List<BoardPosition>();

            foreach (var position in BoardPosition.All)
            {
                if (FindFlips(board, player, position).Count > 0)
                {
                    moves.Add(position);
                }
            }

            return moves;
        }

        /// <summary><paramref name="player"/> に着手できるマスが 1 つでもあるかどうか。</summary>
        public static bool HasLegalMove(Board board, Player player)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            foreach (var position in BoardPosition.All)
            {
                if (FindFlips(board, player, position).Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 1 方向だけを走査し、相手石の連なりが自石で閉じていればそれらを <paramref name="flips"/> に加える。
        /// 盤の端で途切れた場合や、空きマスに当たった場合は何も加えない。
        /// </summary>
        private static void CollectFlipsInDirection(
            Board board,
            BoardPosition move,
            BoardDirection direction,
            CellState own,
            CellState opponent,
            ref List<BoardPosition> flips)
        {
            var current = move;
            var run = 0;
            var closedByOwnDisc = false;

            // 相手石が続く限り進み、自石に当たれば挟めている。
            // 空きマスに当たった場合と盤の端まで来た場合は挟めていない。
            while (current.TryOffset(direction, out var next))
            {
                current = next;
                var state = board[current];

                if (state == opponent)
                {
                    run++;
                    continue;
                }

                closedByOwnDisc = state == own;
                break;
            }

            if (run == 0 || !closedByOwnDisc)
            {
                return;
            }

            if (flips == null)
            {
                flips = new List<BoardPosition>();
            }

            var flipped = move;

            for (var step = 0; step < run; step++)
            {
                flipped.TryOffset(direction, out var nextFlipped);
                flipped = nextFlipped;
                flips.Add(flipped);
            }
        }
    }
}
