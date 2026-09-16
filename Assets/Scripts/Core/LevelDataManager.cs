using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core;
using Engine.ShelfPuzzle;
using JetBrains.Annotations;
using UnityEngine;

namespace Strategy.Level
{
    public class LevelData
    {
        public readonly IShelfItem[][][] ShelveItems;
        public readonly IShelf2[] Shelves;

        public LevelData(
            IShelfItem[][][] shelveItems,
            IShelf2[] shelves
        )
        {
            ShelveItems = shelveItems;
            Shelves = shelves;
        }
    }

    public class LevelDataManager : ILevelDataManager
    {
        /**
         * [shelf] [layer] [slot]
         */
        private readonly IShelfItem[][][] _shelves;

        private readonly IShelf2[] _shelvesObjects;
        private readonly Dictionary<int, IShelfItem> _items;

        public Action OnSlotDataChanged { get; set; }

        public LevelDataManager(LevelData levelData)
        {
            _shelves = levelData.ShelveItems;
            _shelvesObjects = levelData.Shelves;
            _items = new Dictionary<int, IShelfItem>();
            foreach (var layers in _shelves)
            {
                foreach (var layer in layers)
                {
                    foreach (var shelf in layer)
                    {
                        if (shelf != null)
                        {
                            _items[shelf.Meta.Id] = shelf;
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
        }

        public ShelfPuzzleInputData[] Export()
        {
            var export = new ShelfPuzzleInputData[_shelves.Length];
            for (var shelfId = 0; shelfId < _shelves.Length; shelfId++)
            {
                var shelf = _shelvesObjects[shelfId];
                export[shelfId] = new ShelfPuzzleInputData
                {
                    Type = shelf.Type,
                    Data = _shelves[shelfId]
                        .Select(e =>
                            e.Select(p => p != null ? p.Meta.TypeId : 0).ToArray()
                        ).ToArray()
                };
            }

            return export;
        }

        public void RemoveItem(int itemId)
        {
            if (!_items.Remove(itemId, out var item))
            {
                return;
            }

            SetSlotData(item.Meta.ShelfId, item.Meta.LayerId, item.Meta.SlotId, null);
        }

        public List<IShelfItem> GetItems()
        {
            return _items.Values.ToList();
        }

        public IShelfItem GetItem(int shelfId, int layerId, int slotId)
        {
            if (_shelves == null) return null;

            if (shelfId < 0 || shelfId >= _shelves.Length) return null;
            var shelf = _shelves[shelfId];

            if (layerId < 0 || layerId >= shelf.Length) return null;
            var layer = shelf[layerId];

            if (slotId < 0 || slotId >= layer.Length) return null;
            return layer[slotId];
        }

        [CanBeNull]
        public IShelfItem FindItemInShelf(int shelfId, int itemTypeId)
        {
            if (_shelves == null) return null;
            if (shelfId < 0 || shelfId >= _shelves.Length) return null;
            var shelf = _shelves[shelfId];
            var layerId = FindNotEmptyLayer(shelf);
            var layer = shelf[layerId];
            return FindItemTypeInLayer(layer, itemTypeId);
        }

        public IShelfItem FindItem(int itemId)
        {
            return _items[itemId];
        }

        public IShelf2 GetShelf(int shelfId)
        {
            return _shelvesObjects.FirstOrDefault(e => e.Id == shelfId);
        }

        public IShelf2[] GetShelves()
        {
            return _shelvesObjects.ToArray();
        }

        public List<IDragObject> GetDrags()
        {
            return _items.Values.Select(e => e.DragObject).Where(d => d != null).ToList();
        }

        /// Return -1 nếu Shelf không tồn tại
        /// Return 0 nếu Layer trên cùng đang empty
        public int GetTopLayerOfShelf(int shelfId)
        {
            if (shelfId < 0 || shelfId >= _shelves.Length) return -1;
            var shelf = _shelves[shelfId];
            return FindNotEmptyLayer(shelf);
        }

        [CanBeNull]
        public IShelfItem[] GetTopLayer(int shelfId)
        {
            if (shelfId < 0 || shelfId >= _shelves.Length) return null;
            var shelf = _shelves[shelfId];
            foreach (var layer in shelf)
            {
                foreach (var slot in layer)
                {
                    if (slot != null)
                    {
                        return layer;
                    }
                }
            }

            return Array.Empty<IShelfItem>();
        }

        [CanBeNull]
        public IShelfItem[] GetLayer(int shelfId, int layerId)
        {
            if (shelfId < 0 || shelfId >= _shelves.Length) return null;
            var shelf = _shelves[shelfId];
            if (layerId < 0 || layerId >= shelf.Length) return null;
            return shelf[layerId];
        }

        public bool IsLayerEmpty(int shelfId, int layerId)
        {
            if (shelfId < 0 || shelfId >= _shelves.Length) return true;
            var shelf = _shelves[shelfId];
            if (layerId < 0 || layerId >= shelf.Length) return true;
            var layer = shelf[layerId];
            foreach (var slot in layer)
            {
                if (slot != null) return false;
            }

            return true;
        }
        
        /// Return -1 nếu không còn layer nào sau đó
        public int GetNextNonEmptyLayerId(int shelf, int fromLayer)
        {
            while (true)
            {
                var currentLayer = fromLayer + 1;
                var layer = GetLayer(shelf, currentLayer);
                if (layer == null)
                {
                    return -1;
                }

                foreach (var slot in layer)
                {
                    if (slot != null) return currentLayer;
                }

                fromLayer = currentLayer;
            }
        }

        public void SetSlotData(int shelfId, int layerId, int slotId, IShelfItem item)
        {
            if (_shelves == null) return;

            if (shelfId < 0 || shelfId >= _shelves.Length) return;
            var shelf = _shelves[shelfId];

            if (layerId < 0 || layerId >= shelf.Length) return;
            var layer = shelf[layerId];

            if (slotId < 0 || slotId >= layer.Length) return;
            layer[slotId] = item;
            OnSlotDataChanged?.Invoke();
        }

        public bool IsDeadlock()
        {
            // foreach all shelf
            foreach (var shelf in _shelvesObjects)
            {
                if (shelf.Type != ShelfType.Common) continue;
                var topLayer = GetTopLayer(shelf.Id);
                if (topLayer == null || topLayer.Length == 0) return false;
                foreach (var t in topLayer)
                {
                    if (t == null || t.IsGoldenEgg()) return false;
                }
            }
            return true;
        }

        public string PrintLevel()
        {
            var sb = new System.Text.StringBuilder();

            for (var shelfId = 0; shelfId < _shelves.Length; shelfId++)
            {
                var shelf = _shelves[shelfId];
                var layers = shelf.Select(layer =>
                    "[" + string.Join(" ", layer.Select(item => item != null ? item.Meta.TypeId.ToString() : "_")) + "]"
                );
                sb.AppendLine($"Shelf {shelfId}: {string.Join(" ", layers)}");
            }

            return sb.ToString();
        }

        /* Trả về Id của Layer đang empty, nếu tất cả layer đều empty thì trả về 0 - top layer */
        private static int FindNotEmptyLayer(IShelfItem[][] shelf)
        {
            for (var layerId = 0; layerId < shelf.Length; layerId++)
            {
                var layer = shelf[layerId];
                if (layer.Any(t => t != null))
                {
                    return layerId;
                }
            }

            return 0;
        }

        [CanBeNull]
        private static IShelfItem FindItemTypeInLayer(IShelfItem[] layer, int itemTypeId)
        {
            return layer.FirstOrDefault(slot => slot != null && slot.Meta.TypeId == itemTypeId);
        }
    }
}