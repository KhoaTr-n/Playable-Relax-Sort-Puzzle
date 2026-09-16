using Core;
using UnityEngine;

namespace Game
{
    // [CreateAssetMenu(menuName = "Game/Shelf/CommonNormal")]
    public class CommonShelfNormalSpacingData : ScriptableObject, ISpacingData
    {
        [SerializeField] private Vector2[] topLayerSlotsPositions = new[]
        {
            new Vector2(-1, -1),
            new Vector2(0, -1),
            new Vector2(1, -1),
        };

        [SerializeField] private Vector2[] secondLayerSlotsPositions = new[]
        {
            new Vector2(-0.8f, -0.8f),
            new Vector2(0, -0.8f),
            new Vector2(0.8f, -0.8f),
        };

        public Vector2 GetPosition(int layerId, int slotId)
        {
            if (slotId < 0 || slotId >= 3)
            {
                Debug.LogError("Offset null, vui lòng check lại");
                return Vector2.zero;
            }

            if (layerId < 0 || layerId > 1)
            {
                Debug.LogError($"Sai Layer Id: {layerId}");
                return Vector2.zero;
            }

            return layerId == 0 ? topLayerSlotsPositions[slotId] : secondLayerSlotsPositions[slotId];
        }
    }
}