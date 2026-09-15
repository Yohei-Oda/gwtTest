using System;
using Othello.Core;

namespace Othello.View
{
    /// <summary>
    /// 一局面から画面に出す内容を導いた値。手番・スコア・パス通知・勝敗を持つ。
    ///
    /// 局面の解釈はすべて <see cref="GameState"/> に委ね、ここでは「何と書くか」だけを決める。
    /// UnityEngine には依存しないので、表示文言を GameObject 抜きで固定できる。
    /// </summary>
    public readonly struct GameStatus
    {
        private GameStatus(
            Player playerToMove,
            bool isGameOver,
            int blackDiscCount,
            int whiteDiscCount,
            string turnText,
            string passNoticeText,
            string resultText)
        {
            PlayerToMove = playerToMove;
            IsGameOver = isGameOver;
            BlackDiscCount = blackDiscCount;
            WhiteDiscCount = whiteDiscCount;
            TurnText = turnText;
            PassNoticeText = passNoticeText;
            ResultText = resultText;
        }

        /// <summary>手番の対局者。終局していれば意味を持たない。</summary>
        public Player PlayerToMove { get; }

        /// <summary>終局しているかどうか。</summary>
        public bool IsGameOver { get; }

        /// <summary>盤上の黒石の数。</summary>
        public int BlackDiscCount { get; }

        /// <summary>盤上の白石の数。</summary>
        public int WhiteDiscCount { get; }

        /// <summary>手番の表示。終局後は空。</summary>
        public string TurnText { get; }

        /// <summary>パスが起きたことの通知。起きていなければ空。</summary>
        public string PassNoticeText { get; }

        /// <summary>勝敗と最終スコアの表示。対局中は空。</summary>
        public string ResultText { get; }

        /// <summary>黒石の数の表示。</summary>
        public string BlackScoreText
        {
            get { return "黒 " + BlackDiscCount; }
        }

        /// <summary>白石の数の表示。</summary>
        public string WhiteScoreText
        {
            get { return "白 " + WhiteDiscCount; }
        }

        /// <summary>パス通知を出すべきかどうか。</summary>
        public bool HasPassNotice
        {
            get { return PassNoticeText.Length > 0; }
        }

        /// <summary>局面から表示内容を導く。</summary>
        public static GameStatus From(GameState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var blackDiscCount = state.Board.CountOf(Player.Black);
            var whiteDiscCount = state.Board.CountOf(Player.White);

            if (state.IsGameOver)
            {
                // 終局後に手番やパスを出し続けると、まだ続くように見えてしまう。勝敗だけを残す。
                return new GameStatus(
                    state.PlayerToMove,
                    true,
                    blackDiscCount,
                    whiteDiscCount,
                    string.Empty,
                    string.Empty,
                    DescribeResult(state.GetResult()));
            }

            return new GameStatus(
                state.PlayerToMove,
                false,
                blackDiscCount,
                whiteDiscCount,
                "手番: " + NameOf(state.PlayerToMove),
                DescribePass(state.PassedPlayer),
                string.Empty);
        }

        private static string DescribePass(Player? passedPlayer)
        {
            if (passedPlayer == null)
            {
                return string.Empty;
            }

            return NameOf(passedPlayer.Value) + "は打てる手が無いためパスしました";
        }

        private static string DescribeResult(GameResult result)
        {
            var verdict = result.IsDraw ? "引き分け" : NameOf(result.Winner.Value) + "の勝ち";

            return verdict + "  黒 " + result.BlackDiscCount + " - 白 " + result.WhiteDiscCount;
        }

        private static string NameOf(Player player)
        {
            return player == Player.Black ? "黒" : "白";
        }
    }
}
