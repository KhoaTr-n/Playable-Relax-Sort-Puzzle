using System;
using Engine.ShelfPuzzle;
using JetBrains.Annotations;
using UnityEngine;

namespace Core
{
    public interface IShelf2
    {
        /* Shelf ID */
        int Id { get; }
        ShelfType Type { get; }
        ISpacingData SpacingData { get; }
        IDropZone[]  DropZones { get; }
        void Init(int shelfId);
        void OnTopLayerCleared([CanBeNull] Action<Vector2> onCleared);
        
        Bounds GetShelfBounds();
        
        // ============ UI ============
        // For Unity only
        
        object UnityTransform { get; }
        
        // ============ UI ============
        
    }
}