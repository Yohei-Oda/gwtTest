using Othello.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Othello.View
{
    /// <summary>
    /// 手番・スコア・パス通知・勝敗を表示し、リスタートを受け付ける画面まわり。
    ///
    /// 盤面と同じく、表示部品はシーンに焼き付けず <see cref="EnsureInitialized"/> で組み立てる。
    /// 何と書くかは <see cref="GameStatus"/> が決めるので、ここはその結果を Text に流すだけ。
    /// ルール判定は一切持たない。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class GameHudView : MonoBehaviour
    {
        [Header("接続先")]
        [SerializeField] private BoardView board;

        [Header("配色")]
        [SerializeField] private Color panelColor = new Color(0.16f, 0.19f, 0.22f, 1f);
        [SerializeField] private Color textColor = new Color(0.94f, 0.95f, 0.96f, 1f);
        [SerializeField] private Color noticeColor = new Color(1f, 0.82f, 0.36f, 1f);
        [SerializeField] private Color buttonColor = new Color(0.22f, 0.45f, 0.35f, 1f);

        [Header("寸法")]
        [SerializeField] private float panelWidth = 560f;
        [SerializeField] private float panelPadding = 32f;

        private Text turnLabel;
        private Text blackScoreLabel;
        private Text whiteScoreLabel;
        private Text passNoticeLabel;
        private Text resultLabel;
        private Button restartButton;

        private BoardPresenter presenter;
        private bool initialized;

        /// <summary>対局をやり直すボタン。対局中も終局後も押せる。</summary>
        public Button RestartButton
        {
            get { return restartButton; }
        }

        /// <summary>いま表示している手番。終局後は空。</summary>
        public string TurnText
        {
            get { return turnLabel.text; }
        }

        /// <summary>いま表示している黒石の数。</summary>
        public string BlackScoreText
        {
            get { return blackScoreLabel.text; }
        }

        /// <summary>いま表示している白石の数。</summary>
        public string WhiteScoreText
        {
            get { return whiteScoreLabel.text; }
        }

        /// <summary>いま表示しているパス通知。</summary>
        public string PassNoticeText
        {
            get { return passNoticeLabel.text; }
        }

        /// <summary>パス通知が画面に出ているかどうか。</summary>
        public bool IsPassNoticeVisible
        {
            get { return passNoticeLabel.gameObject.activeSelf; }
        }

        /// <summary>いま表示している勝敗と最終スコア。対局中は空。</summary>
        public string ResultText
        {
            get { return resultLabel.text; }
        }

        /// <summary>勝敗表示が画面に出ているかどうか。</summary>
        public bool IsResultVisible
        {
            get { return resultLabel.gameObject.activeSelf; }
        }

        /// <summary>盤面につなぎ、以後その対局の進行を表示する。</summary>
        public void Bind(BoardView boardView)
        {
            EnsureInitialized();

            Unsubscribe();

            board = boardView;

            if (board == null)
            {
                return;
            }

            board.EnsureInitialized();

            presenter = board.Presenter;
            presenter.Changed += Show;

            Show(presenter.State);
        }

        /// <summary>表示部品の GameObject 階層を組み立てる。二度目以降は何もしない。</summary>
        public void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            BuildPanel();
        }

        /// <summary>与えられた局面の内容を表示する。</summary>
        public void Show(GameState state)
        {
            EnsureInitialized();

            var status = GameStatus.From(state);

            turnLabel.text = status.TurnText;

            // 終局後に手番の行だけ空で残ると、盤面がまだ続くように見える。行ごと畳む。
            turnLabel.gameObject.SetActive(status.TurnText.Length > 0);

            blackScoreLabel.text = status.BlackScoreText;
            whiteScoreLabel.text = status.WhiteScoreText;

            passNoticeLabel.text = status.PassNoticeText;
            passNoticeLabel.gameObject.SetActive(status.HasPassNotice);

            resultLabel.text = status.ResultText;
            resultLabel.gameObject.SetActive(status.IsGameOver);
        }

        private void Awake()
        {
            // 盤面と同じ理由で、表示部品はシーンに保存せず再生時に組み立てる。
            // EditMode テストからは Bind を直接呼ぶ。
            if (!Application.isPlaying)
            {
                return;
            }

            Bind(board);
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (presenter == null)
            {
                return;
            }

            presenter.Changed -= Show;
            presenter = null;
        }

        private void BuildPanel()
        {
            var rect = (RectTransform)transform;
            rect.sizeDelta = new Vector2(panelWidth, 0f);

            var background = GetOrAdd<Image>(gameObject);
            background.color = panelColor;
            background.raycastTarget = false;

            var layout = GetOrAdd<VerticalLayoutGroup>(gameObject);
            layout.padding = new RectOffset(
                Mathf.RoundToInt(panelPadding),
                Mathf.RoundToInt(panelPadding),
                Mathf.RoundToInt(panelPadding),
                Mathf.RoundToInt(panelPadding));
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = GetOrAdd<ContentSizeFitter>(gameObject);
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            turnLabel = CreateLabel("Turn", 44, FontStyle.Bold, textColor);
            blackScoreLabel = CreateLabel("BlackScore", 36, FontStyle.Normal, textColor);
            whiteScoreLabel = CreateLabel("WhiteScore", 36, FontStyle.Normal, textColor);
            // パス通知は一番長い文面がパネル幅 560px に 1 行で収まるところまで落とす。
            // 折り返すと語の途中で切れて読みにくい。
            passNoticeLabel = CreateLabel("PassNotice", 26, FontStyle.Normal, noticeColor);
            resultLabel = CreateLabel("Result", 36, FontStyle.Bold, textColor);

            restartButton = CreateRestartButton();

            // 出すものが決まるまでは通知と勝敗を隠しておく。
            passNoticeLabel.gameObject.SetActive(false);
            resultLabel.gameObject.SetActive(false);
        }

        private Text CreateLabel(string name, int fontSize, FontStyle style, Color color)
        {
            var labelObject = new GameObject(name, typeof(RectTransform));
            labelObject.transform.SetParent(transform, false);

            var text = labelObject.AddComponent<Text>();
            text.font = UiFont.Resolve();
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;

            // 高さは行数に合わせて縦レイアウトが決める。固定するとパス通知や勝敗が
            // 折り返したときに隣のボタンへはみ出す。
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            text.text = string.Empty;

            return text;
        }

        private Button CreateRestartButton()
        {
            var buttonObject = new GameObject("RestartButton", typeof(RectTransform));
            buttonObject.transform.SetParent(transform, false);

            var background = buttonObject.AddComponent<Image>();
            background.color = buttonColor;

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(Restart);

            var element = buttonObject.AddComponent<LayoutElement>();
            element.minHeight = 72f;
            element.preferredHeight = 72f;

            var labelObject = new GameObject("Label", typeof(RectTransform));
            labelObject.transform.SetParent(buttonObject.transform, false);

            var labelRect = (RectTransform)labelObject.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var text = labelObject.AddComponent<Text>();
            text.font = UiFont.Resolve();
            text.fontSize = 32;
            text.fontStyle = FontStyle.Bold;
            text.color = textColor;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.text = "もう一度あそぶ";

            return button;
        }

        private void Restart()
        {
            if (presenter == null)
            {
                return;
            }

            presenter.Restart();
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();

            return component != null ? component : target.AddComponent<T>();
        }
    }
}
