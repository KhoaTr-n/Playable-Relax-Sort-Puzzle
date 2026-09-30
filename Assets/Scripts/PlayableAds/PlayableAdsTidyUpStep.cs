using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Engine.ShelfPuzzle;
using Game;
using Strategy.Level;

namespace PlayableAds
{
    /// <summary>
    /// Kiểm tra merge trên tất cả shelf (mỗi shelf chỉ có 1 layer).
    /// Nếu tất cả slots cùng TypeId → bounce animation → destroy items → deactivate shelf → trigger slide.
    /// Nếu tất cả slots rỗng → deactivate shelf → trigger slide (skip bounce/destroy).
    /// </summary>
    public class PlayableAdsTidyUpStep : ILevelAnimationStep
    {
        public bool CanInterrupt => false;

        private readonly PlayableAdsStateControl _control;
        private readonly ILevelDataManager _levelDataManager;
        private readonly PlayableAdsGridData _gridData;
        private readonly IDragDropManager _dragDropManager;
        private readonly Action<int> _onMatchCleared;

        private List<MergeData> _merges;
        private bool _animationsStarted;

        public PlayableAdsTidyUpStep(
            PlayableAdsStateControl control,
            ILevelDataManager levelDataManager,
            PlayableAdsGridData gridData,
            IDragDropManager dragDropManager,
            Action<int> onMatchCleared = null)
        {
            _control = control;
            _levelDataManager = levelDataManager;
            _gridData = gridData;
            _dragDropManager = dragDropManager;
            _onMatchCleared = onMatchCleared;
        }

        public void Enter()
        {
            _merges = FindClearableShelves();

            if (_merges.Count == 0)
            {
                _control.ToDragDrop();
            }
        }

        public void Update(float dt)
        {
            if (_merges == null || _merges.Count == 0) return;

            // Phase 1: bắt đầu bounce animation (skip cho empty shelves)
            if (!_animationsStarted)
            {
                bool hasMatch = false;
                foreach (var merge in _merges)
                {
                    if (merge.IsEmpty)
                    {
                        // Empty shelf — skip bounce, mark done immediately
                        merge.AnimDone = merge.Items.Length;
                        continue;
                    }
                    hasMatch = true;

                    foreach (var item in merge.Items)
                    {
                        var m = merge;
                        item.Bounce(() => m.AnimDone++);
                    }
                }

                if (hasMatch)
                {
                    InGameSoundManager.Instance.PlaySound(AudioEnum.Match);
                }

                _animationsStarted = true;
            }

            // Phase 2: chờ animation xong rồi destroy (skip cho empty shelves)
            var allDone = true;
            foreach (var merge in _merges)
            {
                if (merge.AnimDone < merge.Items.Length)
                {
                    allDone = false;
                    continue;
                }

                if (!merge.Destroyed)
                {
                    if (!merge.IsEmpty)
                    {
                        foreach (var item in merge.Items)
                        {
                            _levelDataManager.RemoveItem(item.Meta.Id);
                            item.DestroyItem(true);
                        }
                    }

                    merge.Destroyed = true;
                    allDone = false;
                }
            }

            if (!allDone) return;

            // Phase 3: deactivate cleared shelves, unregister drop zones, collect slide data
            var allSlides = new List<SlideData>();
            var matchClearCount = 0;
            foreach (var merge in _merges)
            {
                // Chỉ đếm shelf match-3 (không tính shelf trống)
                if (!merge.IsEmpty)
                {
                    matchClearCount++;
                }

                var shelf = _levelDataManager.GetShelf(merge.ShelfId);
                if (shelf != null)
                {
                    // Unregister drop zones để tránh drop vào shelf đã inactive
                    foreach (var dropZone in shelf.DropZones)
                    {
                        _dragDropManager.UnregisterDropZone(dropZone);
                    }

                    var shelfBase = (ShelfBase)shelf;
                    shelfBase.gameObject.SetActive(false);
                }

                var slides = _gridData.OnShelfCleared(merge.ShelfId);
                allSlides.AddRange(slides);
            }

            // Báo số shelf match-3 đã clear trong lượt này
            if (matchClearCount > 0)
            {
                _onMatchCleared?.Invoke(matchClearCount);
            }

            _control.ToSlideDown(allSlides);
        }

        public void Exit()
        {
        }

        /// <summary>
        /// Tìm shelves cần clear: match-3 hoặc tất cả slots rỗng.
        /// </summary>
        private List<MergeData> FindClearableShelves()
        {
            var result = new List<MergeData>();
            var shelves = _levelDataManager.GetShelves();

            foreach (var shelf in shelves)
            {
                if (shelf.Type != ShelfType.Common) continue;

                // Playable ads: luôn dùng layer 0
                var layer = _levelDataManager.GetLayer(shelf.Id, 0);
                if (layer == null) continue;

                // Check all slots empty → clear without animation
                var allEmpty = layer.All(e => e == null);
                if (allEmpty)
                {
                    result.Add(new MergeData(shelf.Id, layer, true));
                    continue;
                }

                // Check match-3
                var first = layer[0];
                if (first == null) continue;

                var canMerge = layer.All(e => e != null && e.Meta.TypeId == first.Meta.TypeId);
                if (canMerge)
                {
                    result.Add(new MergeData(shelf.Id, layer, false));
                }
            }

            return result;
        }

        private class MergeData
        {
            public readonly int ShelfId;
            public readonly IShelfItem[] Items;
            public readonly bool IsEmpty;
            public int AnimDone;
            public bool Destroyed;

            public MergeData(int shelfId, IShelfItem[] items, bool isEmpty)
            {
                ShelfId = shelfId;
                Items = items;
                IsEmpty = isEmpty;
            }
        }
    }
}
