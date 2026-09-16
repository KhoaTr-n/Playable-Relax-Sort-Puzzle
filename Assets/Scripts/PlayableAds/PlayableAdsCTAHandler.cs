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

        public static void TriggerCTA()
        {
            Debug.Log("[PlayableAds] CTA triggered — opening store");

            // 1. Gọi mở Store trước để trigger event "Click on CTA"
            Playable.InstallFullGame();

            // 2. Báo kết thúc màn chơi để trigger event "End of game"
            LifeCycle.GameEnded();
        }
    }
}