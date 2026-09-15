using System;

namespace Othello.Core
{
    /// <summary>
    /// 終局時の勝敗。石数が多い方が勝ち、同数なら引き分け。
    /// </summary>
    public readonly struct GameResult : IEquatable<GameResult>
    {
        /// <summary>終局時の黒石の数。</summary>
        public int BlackDiscCount { get; }

        /// <summary>終局時の白石の数。</summary>
        public int WhiteDiscCount { get; }

        public GameResult(int blackDiscCount, int whiteDiscCount)
        {
            if (blackDiscCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(blackDiscCount), blackDiscCount, "石数は負にならない。");
            }

            if (whiteDiscCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(whiteDiscCount), whiteDiscCount, "石数は負にならない。");
            }

            BlackDiscCount = blackDiscCount;
            WhiteDiscCount = whiteDiscCount;
        }

        /// <summary>石数が同数かどうか。</summary>
        public bool IsDraw
        {
            get { return BlackDiscCount == WhiteDiscCount; }
        }

        /// <summary>勝者。引き分けなら null。</summary>
        public Player? Winner
        {
            get
            {
                if (IsDraw)
                {
                    return null;
                }

                return BlackDiscCount > WhiteDiscCount ? Player.Black : Player.White;
            }
        }

        /// <inheritdoc />
        public bool Equals(GameResult other)
        {
            return BlackDiscCount == other.BlackDiscCount && WhiteDiscCount == other.WhiteDiscCount;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is GameResult other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (BlackDiscCount * 397) ^ WhiteDiscCount;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var verdict = IsDraw ? "Draw" : Winner + " wins";
            return "Black " + BlackDiscCount + " - White " + WhiteDiscCount + " (" + verdict + ")";
        }
    }
}
