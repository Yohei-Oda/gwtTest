using System;
using System.Collections.Generic;

namespace Othello.Core
{
    /// <summary>
    /// 対局の一局面。盤面・手番・合法手・終局判定をひとまとめに持つイミュータブルな値。
    ///
    /// 生成時に手番を正規化するので、<see cref="IsGameOver"/> が false のあいだ
    /// <see cref="PlayerToMove"/> は必ず合法手を 1 つ以上持つ。手番を飛ばされた側は
    /// <see cref="PassedPlayer"/> で分かる。
    /// </summary>
    public sealed class GameState
    {
        private static readonly BoardPosition[] NoMoves = new BoardPosition[0];

        private GameState(
            Board board,
            Player playerToMove,
            bool isGameOver,
            Player? passedPlayer,
            IReadOnlyList<BoardPosition> legalMoves)
        {
            Board = board;
            PlayerToMove = playerToMove;
            IsGameOver = isGameOver;
            PassedPlayer = passedPlayer;
            LegalMoves = legalMoves;
        }

        /// <summary>現在の盤面。</summary>
        public Board Board { get; }

        /// <summary>手番の対局者。<see cref="IsGameOver"/> が true のときは意味を持たない。</summary>
        public Player PlayerToMove { get; }

        /// <summary>双方に合法手が無い、または盤面が埋まっているかどうか。</summary>
        public bool IsGameOver { get; }

        /// <summary>
        /// この局面に至る際に合法手が無くパスになった対局者。パスが無ければ null。
        /// </summary>
        public Player? PassedPlayer { get; }

        /// <summary>手番の対局者が着手できるマス。終局していれば空。</summary>
        public IReadOnlyList<BoardPosition> LegalMoves { get; }

        /// <summary>対局開始局面。初期配置・黒番から始まる。</summary>
        public static GameState CreateInitial()
        {
            return Create(Board.CreateInitial(), Player.Black);
        }

        /// <summary>
        /// 任意の盤面と手番から局面を作る。
        /// <paramref name="playerToMove"/> に合法手が無ければ手番は相手へ渡り、
        /// 双方に合法手が無ければ終局として扱う。
        /// </summary>
        public static GameState Create(Board board, Player playerToMove)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            var moves = OthelloRules.FindLegalMoves(board, playerToMove);

            if (moves.Count > 0)
            {
                return new GameState(board, playerToMove, false, null, moves);
            }

            var opponent = playerToMove.Opponent();
            var opponentMoves = OthelloRules.FindLegalMoves(board, opponent);

            if (opponentMoves.Count > 0)
            {
                return new GameState(board, opponent, false, playerToMove, opponentMoves);
            }

            return new GameState(board, playerToMove, true, null, NoMoves);
        }

        /// <summary>手番の対局者が <paramref name="position"/> に着手できるかどうか。</summary>
        public bool IsLegalMove(BoardPosition position)
        {
            if (IsGameOver)
            {
                return false;
            }

            return OthelloRules.FindFlips(Board, PlayerToMove, position).Count > 0;
        }

        /// <summary>
        /// 着手を適用し、次の局面を <paramref name="next"/> に返す。
        /// 非合法手なら false を返し、<paramref name="next"/> は null になる。
        /// この局面はイミュータブルなので、失敗しても盤面は一切変化しない。
        /// </summary>
        public bool TryPlay(BoardPosition position, out GameState next)
        {
            next = null;

            if (IsGameOver)
            {
                return false;
            }

            var flips = OthelloRules.FindFlips(Board, PlayerToMove, position);

            if (flips.Count == 0)
            {
                return false;
            }

            var disc = PlayerToMove.ToCellState();
            var board = Board.With(position, disc).WithAll(flips, disc);

            next = Create(board, PlayerToMove.Opponent());
            return true;
        }

        /// <summary>
        /// 着手を適用して次の局面を返す。非合法手なら <see cref="InvalidMoveException"/> を投げる。
        /// </summary>
        public GameState Play(BoardPosition position)
        {
            if (!TryPlay(position, out var next))
            {
                throw new InvalidMoveException(position, PlayerToMove);
            }

            return next;
        }

        /// <summary>
        /// 終局時の勝敗を返す。まだ終局していなければ <see cref="InvalidOperationException"/>。
        /// </summary>
        public GameResult GetResult()
        {
            if (!IsGameOver)
            {
                throw new InvalidOperationException("対局はまだ終了していない。");
            }

            return new GameResult(Board.CountOf(Player.Black), Board.CountOf(Player.White));
        }
    }
}
