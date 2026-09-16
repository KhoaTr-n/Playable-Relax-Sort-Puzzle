using UnityEngine;

namespace Core
{
    public interface IDragObject
    {
        /**
         * Unique Id của Item trong Game Scene
         */
        int Id { get; }

        Vector3 Position { get; }

        Vector3 GetOriginalPosition();
        Bounds GetSpriteBounds();
        void UpdatePosition(Vector3 newPosition);
        bool ShouldReturnToOriginal();
        void ReturnToOriginalPosition();
        void ResetVisuals();
        void OnEndDrag();
        bool ContainsPosition(Vector2 position);
        bool CanBeDragged();
        void OnStartDrag();
        void ApplyDragVisuals(Color dragColor, Vector3 dragScale);
    }
}