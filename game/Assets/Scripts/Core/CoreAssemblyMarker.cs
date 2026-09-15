namespace Othello.Core
{
    /// <summary>
    /// Othello.Core アセンブリの疎通確認用マーカー。
    /// ゲームロジックは UnityEngine 非依存の純粋 C# としてこのアセンブリに置く
    /// （Issue #1 / AC-3）。後続 Issue で盤面ロジックがここに追加される。
    /// </summary>
    public static class CoreAssemblyMarker
    {
        /// <summary>このアセンブリの名前。</summary>
        public const string AssemblyName = "Othello.Core";
    }
}
