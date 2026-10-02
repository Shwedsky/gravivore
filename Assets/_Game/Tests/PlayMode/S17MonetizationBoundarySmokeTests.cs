using System.Collections;
using Gravivore.Platform.Monetization;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gravivore.Tests.PlayMode
{
    public sealed class S17MonetizationBoundarySmokeTests
    {
        [UnityTest]
        public IEnumerator CanonicalScene_ComposesDisabledMonetizationWithoutUiOrGameplayChanges()
        {
            var scene = new CanonicalSceneTestScope();
            yield return scene.Load();

            Assert.IsInstanceOf<DisabledPurchaseService>(scene.Root.PurchaseService);
            Assert.IsInstanceOf<DisabledRewardedAdService>(scene.Root.RewardedAdService);
            Assert.IsFalse(scene.Root.PurchaseService.IsAvailable);
            Assert.IsFalse(scene.Root.RewardedAdService.IsAvailable);
            Assert.IsFalse(scene.Root.MonetizationFeatures.ShopEnabled);
            Assert.IsFalse(scene.Root.MonetizationFeatures.RewardedAdsEnabled);
            Assert.IsFalse(scene.Root.MonetizationFeatures.SubscriptionsEnabled);
            Assert.That(scene.Root.Progression.State.TotalAssimilationScore, Is.Zero);

            var buttons = scene.Root.GetComponentsInChildren<Button>(true);
            var forbiddenNames = new[] { "shop", "purchase", "rewarded", "advertisement", "monetization" };
            for (var i = 0; i < buttons.Length; i++)
            {
                var normalizedName = buttons[i].name.ToLowerInvariant();
                for (var j = 0; j < forbiddenNames.Length; j++)
                {
                    Assert.IsFalse(
                        normalizedName.Contains(forbiddenNames[j]),
                        $"Unexpected monetization UI button: {buttons[i].name}");
                }
            }

            yield return scene.Cleanup();
        }
    }
}
