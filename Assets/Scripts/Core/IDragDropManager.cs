using System;

namespace Core
{
    public interface IDragDropManager
    {
        void RegisterDragObject(IDragObject dragObject);
        void UnregisterDragObject(int dragId);
        void RegisterDropZone(DropZoneData dropZone);
        void RegisterItemPickedAction(Action<ShelfItemMeta> onPicked);
        void UnregisterDropZone(IDropZone dropZone);
        void ResetAllObjects();
        void RemoveAll();
        void Pause();
        void Unpause();
        void ManualDropInto(IDragObject dragObject, IDropZone dropZone);
        void Init(Func<IDropZone, bool> canAcceptDropIntoFunc);
        
        bool IsDragging();
    }
}