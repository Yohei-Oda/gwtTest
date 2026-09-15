using System;
using System.Collections.Generic;
using Othello.Core;

namespace Othello.View
{
    /// <summary>
    /// 盤面表示と着手入力をつなぐ表示モデル。
    ///
    /// ルール判定はすべて <see cref="GameState"/> に委ね、ここでは
    /// 「いま何を描けばよいか」と「クリックを局面へ流し込む」ことだけを受け持つ。
    /// UnityEngine には依存しないので、そのまま EditMode テストで動かせる。
    /// </summary>
    public sealed class BoardPresenter
    {
        private readonly HashSet<BoardPosition> hints = new HashSet<BoardPosition>();

        /// <summary>対局開始局面から始める。</summary>
        public BoardPresenter()
            : this(GameState.CreateInitial())
        {
        }

        /// <summary>任意の局面から始める。</summary>
        public BoardPresenter(GameState initialState)
        {
            if (initialState == null)
            {
                throw new ArgumentNullException(nameof(initialState));
            }

            SetState(initialState, false);
        }

        /// <summary>現在の局面。</summary>
        public GameState State { get; private set; }

        /// <summary>局面が進んだときに発火する。引数は新しい局面。</summary>
        public event Action<GameState> Changed;

        /// <summary>指定マスに描くべき石。</summary>
        public CellState GetCell(BoardPosition position)
        {
            return State.Board[position];
        }

        /// <summary>
        /// 指定マスに合法手ヒントを出すべきかどうか。
        /// 手番の対局者の合法手だけが対象なので、終局後はどのマスも false になる。
        /// </summary>
        public bool IsMoveHint(BoardPosition position)
        {
            return hints.Contains(position);
        }

        /// <summary>
        /// クリックされたマスへ着手する。非合法手なら false を返し、局面は一切変化しない。
        /// </summary>
        public bool TryPlay(BoardPosition position)
        {
            if (!State.TryPlay(position, out var next))
            {
                return false;
            }

            SetState(next, true);
            return true;
        }

        private void SetState(GameState state, bool notify)
        {
            State = state;

            hints.Clear();

            for (var i = 0; i < state.LegalMoves.Count; i++)
            {
                hints.Add(state.LegalMoves[i]);
            }

            if (notify && Changed != null)
            {
                Changed(state);
            }
        }
    }
}
