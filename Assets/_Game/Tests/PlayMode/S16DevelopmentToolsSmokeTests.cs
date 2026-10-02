#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using Gravivore.Gameplay.Player;
using NUnit.Framework;
using UnityEngine;
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

        [UnityTest]
        public IEnumerator CanonicalScene_DevelopmentToggleDoesNotOverlapPauseButton()
        {
            var scene = new CanonicalSceneTestScope();
            yield return scene.Load();
            Canvas.ForceUpdateCanvases();

            var developmentToggle = scene.Root.DevelopmentOverlay.ToggleRect;
            var pauseButton = scene.Root.PauseMenu.PauseButtonRect;
            Assert.IsNotNull(developmentToggle);
            Assert.IsNotNull(pauseButton);
            Assert.AreSame(developmentToggle.parent, pauseButton.parent);
            Assert.IsFalse(RectTransformsOverlap(developmentToggle, pauseButton));

            yield return scene.Cleanup();
        }

        private static bool RectTransformsOverlap(RectTransform first, RectTransform second)
        {
            var reference = first.parent;
            var firstBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(reference, first);
            var secondBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(reference, second);
            return firstBounds.min.x < secondBounds.max.x &&
                   firstBounds.max.x > secondBounds.min.x &&
                   firstBounds.min.y < secondBounds.max.y &&
                   firstBounds.max.y > secondBounds.min.y;
        }
    }
}
#endif
