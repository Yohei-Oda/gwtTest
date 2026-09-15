using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Othello.View.EditorTools
{
    /// <summary>
    /// Game.unity の盤面 UI を組み立て直すツール。
    ///
    /// シーンを手で編集する代わりにこれを実行することで、シーンの中身が
    /// コードから追える状態に保たれる。何度実行しても同じ結果になる。
    /// メニュー <c>Othello / Rebuild Game Scene</c>、またはバッチ実行で
    /// <c>-executeMethod Othello.View.EditorTools.GameSceneBuilder.Rebuild</c>。
    /// </summary>
    public static class GameSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Game.unity";
        private const string CanvasName = "UI";
        private const string BoardName = "Board";
        private const string EventSystemName = "EventSystem";

        [MenuItem("Othello/Rebuild Game Scene")]
        public static void Rebuild()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == CanvasName || root.name == EventSystemName)
                {
                    Object.DestroyImmediate(root);
                }
            }

            CreateCanvasWithBoard();
            CreateEventSystem();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            Debug.Log("Game.unity の盤面 UI を再構築した。");
        }

        private static void CreateCanvasWithBoard()
        {
            var canvasObject = new GameObject(
                CanvasName,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            // 盤面は正方形なので、縦を基準に合わせておけば横長画面でも切れない。
            scaler.matchWidthOrHeight = 1f;

            CreateBackdrop(canvasObject.transform);

            // BoardView は再生時にセルを組み立てるので、ここでは空の入れ物だけ置く。
            var boardObject = new GameObject(BoardName, typeof(RectTransform), typeof(BoardView));
            boardObject.transform.SetParent(canvasObject.transform, false);

            var rect = (RectTransform)boardObject.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// 盤面の背後を一色で覆う。既定シーンのスカイボックスが透けると
        /// 盤面が宙に浮いて見えるため、盤面表示の一部として敷いておく。
        /// </summary>
        private static void CreateBackdrop(Transform parent)
        {
            var backdropObject = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
            backdropObject.transform.SetParent(parent, false);

            var rect = (RectTransform)backdropObject.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = backdropObject.GetComponent<Image>();
            image.color = new Color(0.11f, 0.13f, 0.15f, 1f);

            // 盤外のクリックを拾う必要はない。
            image.raycastTarget = false;
        }

        private static void CreateEventSystem()
        {
            // 入力は Input Manager（旧）なので StandaloneInputModule を使う。
            new GameObject(EventSystemName, typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }
}
