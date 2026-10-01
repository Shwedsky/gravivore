#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using Gravivore.Gameplay.Player;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Gravivore.Tests.PlayMode
{
    public sealed class S16DevelopmentToolsSmokeTests
    {
        [UnityTest]
        public IEnumerator CanonicalScene_ComposesGuardedOverlayWithoutMutatingProgression()
        {
            var scene = new CanonicalSceneTestScope();
            yield return scene.Load();
            var levels = scene.Root.PlayerStats.BaseLevels;

            Assert.IsNotNull(scene.Root.DevelopmentOverlay);
            Assert.IsFalse(scene.Root.DevelopmentOverlay.IsVisible);
            scene.Root.DevelopmentOverlay.Toggle();
            Assert.IsTrue(scene.Root.DevelopmentOverlay.IsVisible);
            Assert.That(scene.Root.PlayerStats.BaseLevels.GetLevel(PlayerStatType.Power), Is.EqualTo(levels.Power));
            Assert.That(scene.Root.Progression.State.TotalAssimilationScore, Is.Zero);

            yield return scene.Cleanup();
        }
    }
}
#endif
