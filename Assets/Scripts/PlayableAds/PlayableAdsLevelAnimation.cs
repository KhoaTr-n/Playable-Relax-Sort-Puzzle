using System;
using System.Collections.Generic;
using Core;
using Strategy.Level;

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

        private ILevelAnimationStep _currentStep;

        public PlayableAdsLevelAnimation(
            ILevelDataManager levelDataManager,
            IDragDropManager dragDropManager,
            PlayableAdsGridData gridData)
        {
            _levelDataManager = levelDataManager;
            _dragDropManager = dragDropManager;
            _gridData = gridData;

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
            _currentStep = _dragDropStep;
            _currentStep?.Enter();
        }

        private void SwitchToTidyUp()
        {
            _currentStep?.Exit();
            _currentStep = new PlayableAdsTidyUpStep(
                new PlayableAdsStateControl(SwitchToDragDrop, SwitchToTidyUp, SwitchToSlideDown),
                _levelDataManager,
                _gridData,
                _dragDropManager
            );
            _currentStep.Enter();
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
