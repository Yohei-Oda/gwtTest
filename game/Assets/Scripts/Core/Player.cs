using System;

namespace Othello.Core
{
    /// <summary>
    /// 対局者。オセロは黒先手。
    /// </summary>
    public enum Player
    {
        /// <summary>黒番（先手）。</summary>
        Black = 0,

        /// <summary>白番（後手）。</summary>
        White = 1,
    }

    /// <summary>
    /// <see cref="Player"/> の派生情報。
    /// </summary>
    public static class PlayerExtensions
    {
        /// <summary>相手の対局者を返す。</summary>
        public static Player Opponent(this Player player)
        {
            return player == Player.Black ? Player.White : Player.Black;
        }

        /// <summary>この対局者の石が置かれたマスの状態を返す。</summary>
        public static CellState ToCellState(this Player player)
        {
            switch (player)
            {
                case Player.Black:
                    return CellState.Black;
                case Player.White:
                    return CellState.White;
                default:
                    throw new ArgumentOutOfRangeException(nameof(player), player, "未知の対局者。");
            }
        }
    }
}
