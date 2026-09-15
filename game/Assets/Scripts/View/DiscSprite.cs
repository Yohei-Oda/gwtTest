using UnityEngine;

namespace Othello.View
{
    /// <summary>
    /// 石とヒントマーカーに使う円形スプライトを実行時に生成する。
    ///
    /// 画像アセットを持たずに済ませるためで、生成物はシーンに保存されないよう
    /// <see cref="HideFlags.HideAndDontSave"/> を立てている。用が済んだら
    /// <see cref="Destroy"/> で明示的に破棄すること。
    /// </summary>
    internal static class DiscSprite
    {
        /// <summary>縁を 1px だけぼかした円のスプライトを作る。</summary>
        public static Sprite Create(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "OthelloDisc",
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };

            var centre = (size - 1) * 0.5f;
            var radius = centre;
            var pixels = new Color32[size * size];

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = x - centre;
                    var dy = y - centre;
                    var distance = Mathf.Sqrt((dx * dx) + (dy * dy));

                    // 縁 1px 分でアルファを落としてジャギーを抑える。
                    var alpha = Mathf.Clamp01(radius - distance);

                    pixels[(y * size) + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect);

            sprite.name = "OthelloDisc";
            sprite.hideFlags = HideFlags.HideAndDontSave;

            return sprite;
        }

        /// <summary>スプライトと下地のテクスチャをまとめて破棄する。</summary>
        public static void Destroy(Sprite sprite)
        {
            if (sprite == null)
            {
                return;
            }

            var texture = sprite.texture;

            DestroyObject(sprite);
            DestroyObject(texture);
        }

        private static void DestroyObject(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(target);
            }
            else
            {
                Object.DestroyImmediate(target);
            }
        }
    }
}
