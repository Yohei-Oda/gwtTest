using NUnit.Framework;
using UnityEngine;

namespace Othello.View.Tests
{
    /// <summary>
    /// 画面文字に使うフォント解決の検証（Issue #4 / AC-1〜AC-4 の前提）。
    ///
    /// Text にフォントが載っていないと文字は黙って消える。表示テキストが正しくても
    /// 画面には何も出ないので、フォントが必ず得られることを独立して固定しておく。
    /// </summary>
    public sealed class UiFontTests
    {
        [Test]
        public void Resolve_AlwaysReturnsAUsableFont()
        {
            Assert.That(UiFont.Resolve(), Is.Not.Null);
        }

        [Test]
        public void Resolve_ReturnsTheSameInstanceOnRepeatedCalls()
        {
            Assert.That(UiFont.Resolve(), Is.SameAs(UiFont.Resolve()));
        }

        [Test]
        public void Resolve_PrefersAFontThatCanDrawJapanese()
        {
            var font = UiFont.Resolve();

            // 日本語が出ないフォントしか無い環境でも落とさない。その場合だけ
            // 組み込みフォントへ落ちるので、事実として記録できるようにしておく。
            if (!UiFont.SupportsJapanese)
            {
                Assert.Ignore("日本語を描けるフォントがこの環境に無い（組み込みフォントへフォールバック）。");
            }

            font.RequestCharactersInTexture("手番黒白引き分け勝ち");

            CharacterInfo info;
            Assert.That(font.GetCharacterInfo('手', out info), Is.True, "『手』が描けない。");
            Assert.That(font.GetCharacterInfo('黒', out info), Is.True, "『黒』が描けない。");
        }
    }
}
