using UnityEngine;

namespace Core
{
    public interface ISpacingData
    {
        Vector2 GetPosition(int layerId, int slotId);
    }
}