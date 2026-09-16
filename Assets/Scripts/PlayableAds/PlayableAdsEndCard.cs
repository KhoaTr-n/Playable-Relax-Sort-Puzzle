using DG.Tweening;
using Luna.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace PlayableAds
{
    /// <summary>
    /// Script gắn vào màn hình End Card UI.
    /// Có nhiệm vụ hiển thị overlay và xử lý sự kiện click "Play Now!".
    /// </summary>
    public class PlayableAdsEndCard : MonoBehaviour
    {
        public static PlayableAdsEndCard Instance { get; private set; }
        
        [SerializeField] private Button ctaButton;
        
        private void Awake()
        {
            Instance = this;
            gameObject.SetActive(false); // Ẩn lúc đầu
        }

        public void Show()
        {
            gameObject.SetActive(true);
            
            // Log sự kiện end card
            Analytics.LogEvent(Analytics.EventType.EndCardShown);
            
            if (ctaButton != null)
            {
                DOTween.Kill(ctaButton.transform);
                ctaButton.transform.localScale = Vector3.one;
                ctaButton.transform.DOScale(1.15f, 0.6f)
                    .SetEase(Ease.InOutQuad)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }

        private void OnDisable()
        {
            if (ctaButton != null)
            {
               DOTween.Kill(ctaButton.transform);
            }
        }

        // Gọi hàm này từ sự kiện OnClick của Button "Play Now!"
        public void OnClickPlayNow()
        {
            PlayableAdsCTAHandler.TriggerCTA();
        }
    }
}
