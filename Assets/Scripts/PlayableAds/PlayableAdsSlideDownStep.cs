using System.Collections.Generic;
using DG.Tweening;
using Game;
using Strategy.Level;
using UnityEngine;

namespace PlayableAds
{
    /// <summary>
    /// Animation step: di chuyển (slide) các shelf xuống vị trí mới sau khi shelf bị clear.
    /// Sử dụng DOTween với Ease.OutQuad.
    /// </summary>
    public class PlayableAdsSlideDownStep : ILevelAnimationStep
    {
        public bool CanInterrupt => false;

        private const float SlideDuration = 0.3f;

        private readonly PlayableAdsStateControl _control;
        private readonly ILevelDataManager _levelDataManager;
        private readonly List<SlideData> _slides;
        private int _completedCount;
        private bool _started;

        public PlayableAdsSlideDownStep(
            PlayableAdsStateControl control,
            ILevelDataManager levelDataManager,
            List<SlideData> slides)
        {
            _control = control;
            _levelDataManager = levelDataManager;
            _slides = slides;
        }

        private const float DeltaZ = -0.3f;

        public void Enter()
        {
            if (_slides == null || _slides.Count == 0)
            {
                _control.ToDragDrop();
                return;
            }

            _completedCount = 0;
            _started = true;

            foreach (var slide in _slides)
            {
                var shelf = (ShelfBase)_levelDataManager.GetShelf(slide.ShelfId);
                if (shelf == null)
                {
                    _completedCount++;
                    continue;
                }

                var targetZ = slide.ToRow * DeltaZ;
                var t = shelf.transform;
                t.DOMove(
                        new Vector3(slide.ToPosition.x, slide.ToPosition.y, targetZ),
                        SlideDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => _completedCount++);
            }
        }

        public void Update(float dt)
        {
            if (!_started) return;

            if (_completedCount >= _slides.Count)
            {
                _control.ToDragDrop();
            }
        }

        public void Exit()
        {
        }
    }
}
