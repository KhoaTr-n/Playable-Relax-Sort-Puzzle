using UnityEngine;

namespace Core
{
    public interface IClickObject
    {
        int Id { get; }
        bool CanBeClicked();
        bool ContainsPosition(Vector2 position);
        void OnClicked();
        System.Action<ShelfItemMeta> OnItemPickedAction { get; set; }
    }
}

