using UnityEngine;

namespace Othello.View
{
    /// <summary>
    /// 画面文字に使うフォントの解決。
    ///
    /// 組み込みフォント（LegacyRuntime）は日本語の字形を持たないので、手番や勝敗を
    /// 日本語で出すと何も描かれない。プロジェクトにフォントアセットを持ち込む代わりに、
    /// OS が持つ日本語フォントを実行時に借りる。見つからない環境でも文字が消えるよりは
    /// マシなので、その場合だけ組み込みフォントへ落とす。
    /// </summary>
    public static class UiFont
    {
        /// <summary>日本語を描ける見込みが高い OS フォント。上から順に試す。</summary>
        private static readonly string[] JapaneseFontNames =
        {
            "Hiragino Sans",
            "Hiragino Kaku Gothic ProN",
            "Noto Sans CJK JP",
            "Noto Sans JP",
            "Yu Gothic",
            "Meiryo",
            "MS Gothic",
        };

        private const int FontSize = 48;

        private static Font resolved;

        /// <summary>解決したフォントが日本語を描けるかどうか。</summary>
        public static bool SupportsJapanese { get; private set; }

        /// <summary>画面文字に使うフォントを返す。二度目以降は同じインスタンスを返す。</summary>
        public static Font Resolve()
        {
            if (resolved != null)
            {
                return resolved;
            }

            var installed = Font.GetOSInstalledFontNames();

            foreach (var name in JapaneseFontNames)
            {
                if (!Contains(installed, name))
                {
                    continue;
                }

                var font = Font.CreateDynamicFontFromOSFont(name, FontSize);

                if (font == null)
                {
                    continue;
                }

                resolved = font;
                SupportsJapanese = true;

                return resolved;
            }

            resolved = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            SupportsJapanese = false;

            return resolved;
        }

        private static bool Contains(string[] names, string name)
        {
            if (names == null)
            {
                return false;
            }

            for (var i = 0; i < names.Length; i++)
            {
                if (names[i] == name)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
