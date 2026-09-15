using System;
using System.Linq;
using NUnit.Framework;

namespace Othello.Core.Tests
{
    /// <summary>
    /// 疎通用のスモークテスト。
    /// EditMode テスト基盤が動作し、かつ Othello.Core が UnityEngine 非依存で
    /// あることをバッチモードだけで検証する（Issue #1 / AC-6）。
    /// </summary>
    public sealed class CoreAssemblySmokeTests
    {
        [Test]
        public void CoreAssembly_IsReachableFromEditModeTests()
        {
            Assert.That(CoreAssemblyMarker.AssemblyName, Is.EqualTo("Othello.Core"));
        }

        [Test]
        public void CoreAssembly_DoesNotReferenceUnityEngine()
        {
            var referenced = typeof(CoreAssemblyMarker).Assembly
                .GetReferencedAssemblies()
                .Select(a => a.Name)
                .ToArray();

            Assert.That(
                referenced,
                Has.None.Matches<string>(name =>
                    name.Equals("UnityEngine", StringComparison.Ordinal) ||
                    name.StartsWith("UnityEngine.", StringComparison.Ordinal)),
                "Othello.Core は noEngineReferences: true で UnityEngine から隔離されていなければならない。"
                    + " 実際の参照: " + string.Join(", ", referenced));
        }
    }
}
