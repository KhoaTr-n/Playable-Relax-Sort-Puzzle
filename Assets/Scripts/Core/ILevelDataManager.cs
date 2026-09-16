using System;
using System.Collections.Generic;
using Core;
using Engine.ShelfPuzzle;
using JetBrains.Annotations;

namespace Strategy.Level
{
    public interface ILevelDataManager
    {
        /// <summary>
        /// Fired every time SetSlotData() is called (item moved or removed).
        /// Use this to reset timers that track board inactivity.
        /// </summary>
        Action OnSlotDataChanged { get; set; }
        void Dispose();
        ShelfPuzzleInputData[] Export();
        void RemoveItem(int itemId);
        List<IShelfItem> GetItems();
        IShelfItem GetItem(int shelfId, int layerId, int slotId);
        IShelfItem FindItemInShelf(int shelfId, int itemTypeId);
        IShelfItem FindItem(int itemId);
        IShelf2 GetShelf(int shelfId);
        IShelf2[] GetShelves();
        List<IDragObject> GetDrags();
        
        /// Return -1 nếu Shelf không tồn tại.
        /// Return 0 nếu Layer trên cùng đang empty.
        int GetTopLayerOfShelf(int shelfId);

        [CanBeNull]
        IShelfItem[] GetTopLayer(int shelfId);

        [CanBeNull]
        IShelfItem[] GetLayer(int shelfId, int layerId);

        /// Return -1 nếu không còn layer nào sau đó.
        int GetNextNonEmptyLayerId(int shelf, int fromLayer);

        bool IsLayerEmpty(int shelfId, int layerId);
        void SetSlotData(int shelfId, int layerId, int slotId, IShelfItem item);

        bool IsDeadlock();

        string PrintLevel();
    }
}