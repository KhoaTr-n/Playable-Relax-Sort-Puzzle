using Luna.Unity;
using UnityEngine;

namespace PlayableAds
{
    /// <summary>
    /// Utility tĩnh xử lý CTA (Call-To-Action) cho Playable Ads.
    /// Gọi Luna SDK API để mở Store khi đạt điều kiện.
    /// </summary>
    public static class PlayableAdsCTAHandler
    {
        private static bool _triggered;

        public static void TriggerCta()
        {
            Playable.InstallFullGame();
            LifeCycle.GameEnded();
        }
    }
}