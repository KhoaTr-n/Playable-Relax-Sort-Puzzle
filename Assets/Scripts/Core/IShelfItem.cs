using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Core
{
    public interface IShelfItem
    {
        ShelfItemMeta Meta { get; }
        IDragObject DragObject { get; }
        ShelfLayerDisplay CurrentDisplay { get; }

        void DestroyItem(bool playEffect);

        void SetPosition(Vector3 position);
        void SetShelf(ShelfItemMeta meta, ISpacingData spacingData, ShelfLayerDisplay display);
        void SetNewMeta(ShelfItemMeta newMeta);
        void SetDisplay(ShelfLayerDisplay display);
        void ResetVisual();

        bool IsGoldenEgg();

        // ==== UI functions ====
        /// Chỉ sử dụng cho Unity
        [CanBeNull]
        object UnitySpriteRenderer { get; }

        void FadeInVisual(float duration);
        void Bounce([CanBeNull] Action onCompleted, float delay = 0f);
        void Jiggly(int times = 1);

        // ==== UI functions ====
    }

    public enum ShelfLayerDisplay
    {
        Top,
        Second,
        Hidden
    }
}