using System;

namespace Othello.Core
{
    /// <summary>
    /// 非合法手を <see cref="GameState.Play"/> に渡したときに投げられる。
    /// 戻り値で判別したい場合は <see cref="GameState.TryPlay"/> を使う。
    /// </summary>
    public sealed class InvalidMoveException : Exception
    {
        public InvalidMoveException(BoardPosition position, Player player)
            : base(player + " は " + position + " に着手できない。")
        {
            Position = position;
            Player = player;
        }

        /// <summary>拒否された着手位置。</summary>
        public BoardPosition Position { get; }

        /// <summary>着手しようとした対局者。</summary>
        public Player Player { get; }
    }
}
