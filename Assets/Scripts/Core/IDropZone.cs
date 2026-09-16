using UnityEngine;

namespace Core
{
    public interface IDropZone
    {
        int ShelfId { get; }
        int SlotId { get; }
        bool ShouldSnapToCenter();
        Vector3 GetSnapPosition(int slot);
        bool ContainsPosition(Vector2 position);
    }
}