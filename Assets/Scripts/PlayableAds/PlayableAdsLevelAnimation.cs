using System;
using System.Collections.Generic;
using Core;
using Strategy.Level;
using UnityEngine;

namespace PlayableAds
{
    /// <summary>
    /// State control cho Playable Ads animation.
    /// 3 states: DragDrop ↔ TidyUp ↔ SlideDown
    /// </summary>
    public class PlayableAdsStateControl
    {
        public readonly Action ToDragDrop;
        public readonly Action ToTidyUp;
        public readonly Action<List<SlideData>> ToSlideDown;

        public PlayableAdsStateControl(
            Action toDragDrop,
            Action toTidyUp,
            Action<List<SlideData>> toSlideDown)
        {
            ToDragDrop = toDragDrop;
            ToTidyUp = toTidyUp;
            ToSlideDown = toSlideDown;
        }
    }

    /// <summary>
    /// State machine cho Playable Ads.
    /// Flow: DragDrop → (user drops) → TidyUp → (clear + slide data) → SlideDown → DragDrop
    /// </summary>
    public class PlayableAdsLevelAnimation
    {
        private readonly ILevelDataManager _levelDataManager;
        private readonly IDragDropManager _dragDropManager;
        private readonly PlayableAdsGridData _gridData;
        private readonly PlayableAdsDragDropStep _dragDropStep;
        private readonly int _ctaThreshold;

        private ILevelAnimationStep _currentStep;
        private int _completedMatchCount;

        public PlayableAdsLevelAnimation(
            ILevelDataManager levelDataManager,
            IDragDropManager dragDropManager,
            PlayableAdsGridData gridData,
            int ctaThreshold = 5)
        {
            _levelDataManager = levelDataManager;
            _dragDropManager = dragDropManager;
            _gridData = gridData;
            _ctaThreshold = ctaThreshold;

            var control = new PlayableAdsStateControl(
                SwitchToDragDrop,
                SwitchToTidyUp,
                SwitchToSlideDown
            );

            _currentStep = _dragDropStep = new PlayableAdsDragDropStep(
                control, levelDataManager, dragDropManager
            );
        }

        public void Enter()
        {
            _currentStep?.Enter();
        }

        public void Update(float dt)
        {
            _currentStep?.Update(dt);
        }

        public void Dispose()
        {
            _currentStep = null;
        }

        private void SwitchToDragDrop()
        {
            _currentStep?.Exit();
            if (_completedMatchCount >= _ctaThreshold)
            {
                TriggerEndGame();
                _currentStep = null; // Trở về trạng thái Idle an toàn
                return;
            }
            _currentStep = _dragDropStep;
            _currentStep?.Enter();
        }

        private void TriggerEndGame()
        {
            _dragDropManager.Pause();
                
            if (PlayableAdsEndCard.Instance != null)
            {
                PlayableAdsEndCard.Instance.Show();
            }
            else
            {
                // Fallback nếu quên gắn UI End Card
                PlayableAdsCTAHandler.TriggerCta();
            }
        }

        private void SwitchToTidyUp()
        {
            _currentStep?.Exit();
            _currentStep = new PlayableAdsTidyUpStep(
                new PlayableAdsStateControl(SwitchToDragDrop, SwitchToTidyUp, SwitchToSlideDown),
                _levelDataManager,
                _gridData,
                _dragDropManager,
                OnMatchCleared
            );
            _currentStep.Enter();
        }

        /// <summary>
        /// Callback từ TidyUpStep khi có shelf match-3 được clear.
        /// </summary>
        private void OnMatchCleared(int count)
        {
            _completedMatchCount += count;
            UnityEngine.Debug.Log($"[PlayableAds] Match-3 cleared: +{count}, total: {_completedMatchCount}/{_ctaThreshold}");
            
            // Log analytics
            Luna.Unity.Analytics.LogEvent("match_cleared", _completedMatchCount);
        }

        private void SwitchToSlideDown(List<SlideData> slides)
        {
            _currentStep?.Exit();
            _currentStep = new PlayableAdsSlideDownStep(
                new PlayableAdsStateControl(SwitchToDragDrop, SwitchToTidyUp, SwitchToSlideDown),
                _levelDataManager,
                slides
            );
            _currentStep.Enter();
        }
    }
}
