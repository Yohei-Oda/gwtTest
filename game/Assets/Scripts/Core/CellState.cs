namespace Othello.Core
{
    /// <summary>
    /// 盤面 1 マスの状態。
    /// </summary>
    public enum CellState
    {
        /// <summary>石が置かれていない。</summary>
        Empty = 0,

        /// <summary>黒石が置かれている。</summary>
        Black = 1,

        /// <summary>白石が置かれている。</summary>
        White = 2,
    }
}
