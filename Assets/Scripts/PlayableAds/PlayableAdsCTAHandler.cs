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

        public static void TriggerCTA()
        {
            if (_triggered) return;
            _triggered = true;

            Debug.Log("[PlayableAds] CTA triggered — opening store");

            Luna.Unity.LifeCycle.GameEnded();
            Luna.Unity.Playable.InstallFullGame();
        }
    }
}
