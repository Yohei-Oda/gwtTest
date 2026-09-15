using Othello.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Othello.View
{
    /// <summary>
    /// 盤面 1 マスの表示。石とヒントマーカーの見た目だけを持ち、ルール判定はしない。
    /// 何を描くかは <see cref="BoardView"/> が <see cref="Render"/> で与える。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class BoardCellView : MonoBehaviour
    {
        [SerializeField] private Image disc;
        [SerializeField] private Image hint;
        [SerializeField] private Button button;
        [SerializeField] private int column;
        [SerializeField] private int row;

        /// <summary>このセルが表す盤面座標。</summary>
        public BoardPosition Position
        {
            get { return new BoardPosition(column, row); }
        }

        /// <summary>クリックを受け取るボタン。</summary>
        public Button Button
        {
            get { return button; }
        }

        /// <summary>石が描かれているかどうか。</summary>
        public bool IsDiscVisible
        {
            get { return disc.enabled; }
        }

        /// <summary>いま描かれている石の色。</summary>
        public Color DiscColor
        {
            get { return disc.color; }
        }

        /// <summary>合法手ヒントが描かれているかどうか。</summary>
        public bool IsHintVisible
        {
            get { return hint.enabled; }
        }

        internal void Bind(BoardPosition position, Image discImage, Image hintImage, Button clickTarget)
        {
            column = position.Column;
            row = position.Row;
            disc = discImage;
            hint = hintImage;
            button = clickTarget;
        }

        internal void Render(CellState state, bool isHint, Color blackColor, Color whiteColor)
        {
            disc.enabled = state != CellState.Empty;

            if (disc.enabled)
            {
                disc.color = state == CellState.Black ? blackColor : whiteColor;
            }

            // 石が乗っているマスにヒントは出さない（手番側の合法手は必ず空きマス）。
            hint.enabled = isHint;
        }
    }
}
