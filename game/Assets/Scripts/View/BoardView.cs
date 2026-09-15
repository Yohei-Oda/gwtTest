using System.Collections.Generic;
using Othello.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Othello.View
{
    /// <summary>
    /// 8x8 の盤面を uGUI で描き、セルのクリックを着手として <see cref="BoardPresenter"/> へ流す。
    ///
    /// セルはシーンに置かず <see cref="EnsureInitialized"/> で組み立てる。盤面の見た目が
    /// 64 個の GameObject としてシーンファイルに焼き付かないので、差分が読める状態を保てる。
    /// ルール判定は一切持たず、合法性の判断はすべて <see cref="BoardPresenter"/> 経由で
    /// <see cref="Othello.Core"/> に委ねる。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class BoardView : MonoBehaviour
    {
        private const int Size = BoardPosition.BoardSize;

        [Header("配色")]
        [SerializeField] private Color boardColor = new Color(0.04f, 0.29f, 0.18f, 1f);
        [SerializeField] private Color cellColor = new Color(0.09f, 0.47f, 0.29f, 1f);
        [SerializeField] private Color blackDiscColor = new Color(0.07f, 0.08f, 0.09f, 1f);
        [SerializeField] private Color whiteDiscColor = new Color(0.96f, 0.96f, 0.94f, 1f);
        [SerializeField] private Color hintColor = new Color(1f, 1f, 1f, 0.35f);

        [Header("寸法")]
        [SerializeField] private float cellSize = 96f;
        [SerializeField] private float cellSpacing = 4f;
        [SerializeField, Range(0.1f, 1f)] private float discScale = 0.82f;
        [SerializeField, Range(0.1f, 1f)] private float hintScale = 0.3f;

        private readonly List<BoardCellView> cells = new List<BoardCellView>();

        private BoardCellView[] cellsByIndex;
        private BoardPresenter presenter;
        private Sprite discSprite;
        private bool initialized;

        /// <summary>この盤面が描いている表示モデル。</summary>
        public BoardPresenter Presenter
        {
            get { return presenter; }
        }

        /// <summary>生成済みのセル。a8 から h1 まで表示順に並ぶ。</summary>
        public IReadOnlyList<BoardCellView> Cells
        {
            get { return cells; }
        }

        /// <summary>黒石の描画色。</summary>
        public Color BlackDiscColor
        {
            get { return blackDiscColor; }
        }

        /// <summary>白石の描画色。</summary>
        public Color WhiteDiscColor
        {
            get { return whiteDiscColor; }
        }

        /// <summary>指定座標のセル。</summary>
        public BoardCellView GetCell(BoardPosition position)
        {
            return cellsByIndex[IndexOf(position)];
        }

        /// <summary>盤面の GameObject 階層を組み立てる。二度目以降は何もしない。</summary>
        public void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            presenter = new BoardPresenter();
            presenter.Changed += OnStateChanged;

            discSprite = DiscSprite.Create(128);

            BuildBoard();
            Render();
        }

        private void Awake()
        {
            // エディタで組み立てるとセル 64 個がシーンに保存されてしまうので、再生時だけ作る。
            // EditMode テストからは EnsureInitialized を直接呼ぶ。
            if (!Application.isPlaying)
            {
                return;
            }

            EnsureInitialized();
        }

        private void OnDestroy()
        {
            if (presenter != null)
            {
                presenter.Changed -= OnStateChanged;
            }

            DiscSprite.Destroy(discSprite);
            discSprite = null;
        }

        private void BuildBoard()
        {
            var padding = Mathf.RoundToInt(cellSpacing);
            var extent = (Size * cellSize) + ((Size + 1) * cellSpacing);

            var rect = (RectTransform)transform;
            rect.sizeDelta = new Vector2(extent, extent);

            var background = GetOrAdd<Image>(gameObject);
            background.color = boardColor;
            background.sprite = null;

            var grid = GetOrAdd<GridLayoutGroup>(gameObject);
            grid.cellSize = new Vector2(cellSize, cellSize);
            grid.spacing = new Vector2(cellSpacing, cellSpacing);
            grid.padding = new RectOffset(padding, padding, padding, padding);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Size;

            cells.Clear();
            cellsByIndex = new BoardCellView[Size * Size];

            // 画面では 8 段目が一番上に来るので、段番号は 7 から 0 へ下る。
            for (var row = Size - 1; row >= 0; row--)
            {
                for (var column = 0; column < Size; column++)
                {
                    var cell = CreateCell(new BoardPosition(column, row));

                    cells.Add(cell);
                    cellsByIndex[IndexOf(cell.Position)] = cell;
                }
            }
        }

        private BoardCellView CreateCell(BoardPosition position)
        {
            var cellObject = new GameObject("Cell_" + position, typeof(RectTransform));
            cellObject.transform.SetParent(transform, false);

            var background = cellObject.AddComponent<Image>();
            background.color = cellColor;

            var button = cellObject.AddComponent<Button>();
            button.targetGraphic = background;

            // ヒントは石の下。石が置かれた瞬間にヒントが消えるので重なりは起きない。
            var hint = CreateOverlay(cellObject.transform, "Hint", hintScale, hintColor);
            var disc = CreateOverlay(cellObject.transform, "Disc", discScale, whiteDiscColor);

            var cell = cellObject.AddComponent<BoardCellView>();
            cell.Bind(position, disc, hint, button);

            var clicked = position;
            button.onClick.AddListener(() => OnCellClicked(clicked));

            return cell;
        }

        private Image CreateOverlay(Transform parent, string name, float scale, Color color)
        {
            var overlayObject = new GameObject(name, typeof(RectTransform));
            overlayObject.transform.SetParent(parent, false);

            var margin = (1f - scale) * 0.5f;
            var rect = (RectTransform)overlayObject.transform;
            rect.anchorMin = new Vector2(margin, margin);
            rect.anchorMax = new Vector2(1f - margin, 1f - margin);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = overlayObject.AddComponent<Image>();
            image.sprite = discSprite;
            image.color = color;
            image.preserveAspect = true;

            // クリックはセル本体で受けるので、重ねた画像はレイキャストから外す。
            image.raycastTarget = false;
            image.enabled = false;

            return image;
        }

        private void OnCellClicked(BoardPosition position)
        {
            // 非合法手なら presenter が false を返し、Changed も発火しないので表示は据え置かれる。
            presenter.TryPlay(position);
        }

        private void OnStateChanged(GameState state)
        {
            Render();
        }

        private void Render()
        {
            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                var position = cell.Position;

                cell.Render(
                    presenter.GetCell(position),
                    presenter.IsMoveHint(position),
                    blackDiscColor,
                    whiteDiscColor);
            }
        }

        private static int IndexOf(BoardPosition position)
        {
            return (position.Row * Size) + position.Column;
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();

            return component != null ? component : target.AddComponent<T>();
        }
    }
}
