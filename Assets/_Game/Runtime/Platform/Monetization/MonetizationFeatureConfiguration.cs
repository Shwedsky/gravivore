namespace Gravivore.Platform.Monetization
{
    public sealed class MonetizationFeatureConfiguration
    {
        public MonetizationFeatureConfiguration(
            bool shopEnabled = false,
            bool rewardedAdsEnabled = false,
            bool subscriptionsEnabled = false)
        {
            ShopEnabled = shopEnabled;
            RewardedAdsEnabled = rewardedAdsEnabled;
            SubscriptionsEnabled = subscriptionsEnabled;
        }

        public bool ShopEnabled { get; }
        public bool RewardedAdsEnabled { get; }
        public bool SubscriptionsEnabled { get; }

        public static MonetizationFeatureConfiguration Disabled() =>
            new MonetizationFeatureConfiguration();
    }
}
