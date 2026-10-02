using System;
using System.IO;
using System.Threading.Tasks;
using Gravivore.Platform.Monetization;
using NUnit.Framework;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class S17MonetizationBoundaryTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase(" product")]
        [TestCase("product ")]
        public void ProductId_RejectsEmptyOrUnstableValues(string value)
        {
            Assert.Throws<ArgumentException>(() => new ProductId(value));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase(" entitlement")]
        [TestCase("entitlement ")]
        public void EntitlementId_RejectsEmptyOrUnstableValues(string value)
        {
            Assert.Throws<ArgumentException>(() => new EntitlementId(value));
        }

        [Test]
        public async Task DisabledPurchaseService_IsUnavailableAndNeverGrantsEntitlement()
        {
            var service = new DisabledPurchaseService();
            var entitlement = new EntitlementId("permanent.ad-skip");

            var purchase = await service.PurchaseAsync(new ProductId("permanent.ad-skip"));
            var restore = await service.RestorePurchasesAsync();

            Assert.IsFalse(service.IsAvailable);
            Assert.IsFalse(service.HasEntitlement(entitlement));
            Assert.That(purchase.Status, Is.EqualTo(PurchaseStatus.Unavailable));
            Assert.IsFalse(purchase.Succeeded);
            Assert.IsFalse(purchase.HasGrantedEntitlement);
            Assert.That(restore.Status, Is.EqualTo(PurchaseStatus.Unavailable));
            Assert.IsFalse(restore.HasGrantedEntitlement);
        }

        [Test]
        public async Task DisabledRewardedAdService_IsUnavailableAndNeverGrantsReward()
        {
            var service = new DisabledRewardedAdService();

            var result = await service.ShowRewardedAsync();

            Assert.IsFalse(service.IsAvailable);
            Assert.That(result.Status, Is.EqualTo(RewardedAdStatus.Unavailable));
            Assert.IsFalse(result.RewardEarned);
        }

        [Test]
        public void DefaultResults_AreSafeAndDoNotGrantValue()
        {
            Assert.That(default(PurchaseResult).Status, Is.EqualTo(PurchaseStatus.Unavailable));
            Assert.IsFalse(default(PurchaseResult).Succeeded);
            Assert.IsFalse(default(PurchaseResult).HasGrantedEntitlement);
            Assert.That(default(RewardedAdResult).Status, Is.EqualTo(RewardedAdStatus.Unavailable));
            Assert.IsFalse(default(RewardedAdResult).RewardEarned);
            Assert.Throws<ArgumentException>(() => PurchaseResult.Success(default));
        }

        [Test]
        public void MonetizationFeatureConfiguration_DefaultsAllFeaturesOff()
        {
            var configuration = new MonetizationFeatureConfiguration();

            Assert.IsFalse(configuration.ShopEnabled);
            Assert.IsFalse(configuration.RewardedAdsEnabled);
            Assert.IsFalse(configuration.SubscriptionsEnabled);
        }

        [Test]
        public void RuntimeAndPackages_DoNotReferenceMonetizationVendorsOrSdks()
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            var runtimeRoot = Path.Combine(projectRoot, "Assets", "_Game", "Runtime");
            var forbiddenRuntimeReferences = new[]
            {
                "using RuStore",
                "using Google.Play.Billing",
                "UnityEngine.Purchasing",
                "Unity.Services.Mediation",
                "GoogleMobileAds"
            };

            foreach (var path in Directory.GetFiles(runtimeRoot, "*.cs", SearchOption.AllDirectories))
            {
                var source = File.ReadAllText(path);
                for (var i = 0; i < forbiddenRuntimeReferences.Length; i++)
                {
                    StringAssert.DoesNotContain(forbiddenRuntimeReferences[i], source, path);
                }
            }

            var manifest = File.ReadAllText(Path.Combine(projectRoot, "Packages", "manifest.json")).ToLowerInvariant();
            StringAssert.DoesNotContain("rustore", manifest);
            StringAssert.DoesNotContain("unity.ads", manifest);
            StringAssert.DoesNotContain("firebase", manifest);
            StringAssert.DoesNotContain("purchasing", manifest);
        }
    }
}
