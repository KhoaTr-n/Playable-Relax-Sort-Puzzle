using System.Collections.Generic;
using System.Linq;
using Strategy.Level;
using UnityEngine;

namespace Core
{
    public class DropZone2 : MonoBehaviour, IDropZone
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float detectionRadius = 2.0f;

        [Header("Zone Settings")] //
        [SerializeField]
        private bool isActive = true;

        [SerializeField] private int maxObjects = 1;
        [SerializeField] private bool snapToCenter = true;

        [Header("Custom Bounds")] //
        [SerializeField]
        private Vector2 customSize = new(1f, 1f);

        public int ShelfId { get; private set; }
        public int SlotId { get; private set; }

        private IShelf2 _parentShelf;
        private ILevelDataManager _levelDataManager;

        public void Init(int shelfId, int slotId)
        {
            ShelfId = shelfId;
            SlotId = slotId;
        }

        public void SetDependencies(IShelf2 parentShelf, ILevelDataManager levelDataManager)
        {
            _parentShelf = parentShelf;
            _levelDataManager = levelDataManager;
        }

        // Called by manager
        public bool ContainsPosition(Vector2 position)
        {
            if (!isActive) return false;

            var bounds = GetBounds();
            if (bounds.Contains(position)) return true;

            var distance = Vector2.Distance(transform.position, position);
            return distance < detectionRadius;
        }

        // Getters
        public Vector3 GetSnapPosition(int slot)
        {
            var offset = new Vector3(slot, 0, 0);
            return transform.position + offset;
        }

        public float GetDetectionDistance(Vector2 position)
        {
            var distance = Vector2.Distance(transform.position, position);
            return distance;
        }

        public float GetOverlapArea(Bounds objectBounds)
        {
            var zoneBounds = GetBounds();

            // Tính intersection bounds
            var minX = Mathf.Max(zoneBounds.min.x, objectBounds.min.x);
            var maxX = Mathf.Min(zoneBounds.max.x, objectBounds.max.x);
            var minY = Mathf.Max(zoneBounds.min.y, objectBounds.min.y);
            var maxY = Mathf.Min(zoneBounds.max.y, objectBounds.max.y);

            // Nếu không overlap
            if (minX >= maxX || minY >= maxY) return 0f;

            // Tính diện tích overlap
            var width = maxX - minX;
            var height = maxY - minY;
            return width * height;
        }

        public bool ShouldSnapToCenter() => snapToCenter;
        public void SetActive(bool active) => isActive = active;

        #region Ver2 - Shelf-based detection

        /// <summary>
        /// Lấy bounds của toàn bộ shelf (không phải từng slot)
        /// </summary>
        public Bounds GetShelfBounds_Ver2()
        {
            return _parentShelf?.GetShelfBounds() ?? GetBounds();
        }

        /// <summary>
        /// Tính overlap area với toàn bộ shelf
        /// </summary>
        public float GetShelfOverlapArea(Bounds objectBounds)
        {
            var shelfBounds = GetShelfBounds_Ver2();
        
            var minX = Mathf.Max(shelfBounds.min.x, objectBounds.min.x);
            var maxX = Mathf.Min(shelfBounds.max.x, objectBounds.max.x);
            var minY = Mathf.Max(shelfBounds.min.y, objectBounds.min.y);
            var maxY = Mathf.Min(shelfBounds.max.y, objectBounds.max.y);
        
            if (minX >= maxX || minY >= maxY) return 0f;
        
            return (maxX - minX) * (maxY - minY);
        }

        /// <summary>
        /// Tìm slot trống đầu tiên trong shelf này
        /// </summary>
        /// <returns>SlotId của slot trống, hoặc -1 nếu không có slot trống</returns>
        public int GetFirstEmptySlot()
        {
            if (_levelDataManager == null || _parentShelf == null) return -1;

            var topLayerId = _levelDataManager.GetTopLayerOfShelf(ShelfId);
            var topLayer = _levelDataManager.GetLayer(ShelfId, topLayerId);
            if (topLayer == null) return -1;

            for (var slotId = 0; slotId < topLayer.Length; slotId++)
            {
                if (topLayer[slotId] == null) return slotId;
            }

            return -1;
        }

        /// <summary>
        /// Lấy DropZone của slot trống đầu tiên
        /// </summary>
        public IDropZone GetFirstEmptyDropZone()
        {
            var emptySlotId = GetFirstEmptySlot();
            if (emptySlotId < 0 || _parentShelf == null) return null;

            var dropZones = _parentShelf.DropZones;
            if (emptySlotId >= dropZones.Length) return null;

            return dropZones[emptySlotId];
        }

        /// <summary>
        /// Tìm slot trống gần vị trí thả nhất
        /// </summary>
        /// <param name="dropPosition">Vị trí thả item</param>
        /// <returns>DropZone của slot trống gần nhất, hoặc null nếu không có slot trống</returns>
        public IDropZone GetNearestEmptyDropZone(Vector2 dropPosition)
        {
            if (_levelDataManager == null || _parentShelf == null) return null;

            var topLayerId = _levelDataManager.GetTopLayerOfShelf(ShelfId);
            var topLayer = _levelDataManager.GetLayer(ShelfId, topLayerId);
            if (topLayer == null) return null;

            var dropZones = _parentShelf.DropZones;

            // Tìm tất cả slot trống và tính khoảng cách
            IDropZone nearestZone = null;
            var minDistance = float.MaxValue;

            for (var slotId = 0; slotId < topLayer.Length; slotId++)
            {
                // Chỉ xét slot trống
                if (topLayer[slotId] != null) continue;
                if (slotId >= dropZones.Length) continue;

                var zone = dropZones[slotId];
                var zonePosition = zone.GetSnapPosition(0);
                var distance = Vector2.Distance(dropPosition, zonePosition);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestZone = zone;
                }
            }

            return nearestZone;
        }

        #endregion

        // Helper method to get bounds
        private Bounds GetBounds()
        {
            if (spriteRenderer)
            {
                return spriteRenderer.bounds;
            }

            // Use custom bounds centered at transform position
            var bounds = new Bounds(transform.position, customSize);
            return bounds;
        }

        // Debug visualization
        private void OnDrawGizmos()
        {
            Gizmos.color = isActive ? Color.green : Color.red;

            var bounds = GetBounds();
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}