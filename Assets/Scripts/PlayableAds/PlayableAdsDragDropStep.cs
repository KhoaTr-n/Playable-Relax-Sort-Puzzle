using Core;
using Game;
using Strategy.Level;
using UnityEngine;

namespace PlayableAds
{
    /// <summary>
    /// Phiên bản DragDrop đơn giản cho Playable Ads.
    /// Khác DragDrop gốc:
    /// - Không gọi TidyUp khi pick item (1 layer, không cần reveal)
    /// - Sau drop gọi ToTidyUp thay vì ToMergeLayer
    /// - Không có star/egg effect
    /// </summary>
    public class PlayableAdsDragDropStep : ILevelAnimationStep
    {
        public bool CanInterrupt => true;

        private readonly PlayableAdsStateControl _control;
        private readonly ILevelDataManager _levelDataManager;
        private readonly IDragDropManager _dragDropManager;

        public PlayableAdsDragDropStep(
            PlayableAdsStateControl control,
            ILevelDataManager levelDataManager,
            IDragDropManager dragDropManager)
        {
            _control = control;
            _levelDataManager = levelDataManager;
            _dragDropManager = dragDropManager;

            // Đăng ký drop zones cho tất cả shelf
            foreach (var shelf in _levelDataManager.GetShelves())
            {
                for (var slotId = 0; slotId < shelf.DropZones.Length; slotId++)
                {
                    var sId = slotId;
                    var dropZone = shelf.DropZones[slotId];

                    if (dropZone is DropZone2 dropZone2)
                    {
                        dropZone2.SetDependencies(shelf, _levelDataManager);
                    }

                    _dragDropManager.RegisterDropZone(new DropZoneData
                    {
                        Zone = dropZone,
                        OnDropped = itemId => OnDropped(itemId, shelf.Id, sId)
                    });
                }
            }

            // Đăng ký drag objects
            foreach (var drag in _levelDataManager.GetDrags())
            {
                _dragDropManager.RegisterDragObject(drag);
            }
        }

        public void Enter()
        {
            _dragDropManager.Unpause();
        }

        public void Update(float dt)
        {
        }

        public void Exit()
        {
            _dragDropManager.Pause();
        }

        private void OnDropped(int itemId, int shelfId, int slotId)
        {
            const int layerId = 0; // Playable ads: luôn layer 0
            var slotData = _levelDataManager.GetItem(shelfId, layerId, slotId);

            if (slotData == null)
            {
                var item = (ShelfItemBasic)_levelDataManager.FindItem(itemId);
                var shelf = (ShelfBase)_levelDataManager.GetShelf(shelfId);

                item.gameObject.SetActive(false);
                item.transform.SetParent(shelf.transform, false);

                // Xoá item khỏi shelf cũ
                var prevShelfId = item.Meta.ShelfId;
                var prevLayerId = item.Meta.LayerId;
                var prevSlotId = item.Meta.SlotId;
                _levelDataManager.SetSlotData(prevShelfId, prevLayerId, prevSlotId, null);

                // Đặt item vào shelf mới
                item.SetShelf(
                    item.Meta.Change(shelfId, layerId, slotId),
                    shelf.SpacingData,
                    ShelfLayerDisplay.Top);
                item.ResetVisual();
                item.Jiggly();

                _levelDataManager.SetSlotData(shelfId, layerId, slotId, item);

                // Chuyển sang TidyUp để check merge
                _control.ToTidyUp();
            }
            else
            {
                if (slotData.Meta.Id != itemId)
                {
                    Debug.LogError("Slot đã có item, không thể place thêm vào");
                }
            }
        }
    }
}
