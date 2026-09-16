using System;
using Core;
using Engine.ShelfPuzzle;
using UnityEngine;

namespace Game
{
    public class CommonShelfNormal : ShelfBase
    {
        [SerializeField] public DropZone2[] dropZone;
        [SerializeField] private SpriteRenderer sprRenderer;
        [SerializeField] private CommonShelfNormalSpacingData spacingData;

        public override int Id { get; protected set; }
        public override ShelfType Type => ShelfType.Common;
        public override ISpacingData SpacingData => spacingData;
        public override IDropZone[] DropZones { get; protected set; }

        public override object UnityTransform => transform;

        private void Awake()
        {
            if (dropZone.Length != 3)
            {
                Debug.LogError("Phải có đúng 3 Drop zone");
            }
        }

        public override void Init(int shelfId)
        {
            Id = shelfId;
            name = $"{name}-{shelfId}";
            
            DropZones = new IDropZone[dropZone.Length];
            for (var slotId = 0; slotId < dropZone.Length; slotId++)
            {
                var zone = dropZone[slotId];
                zone.Init(Id, slotId);
                DropZones[slotId] = zone;
            }
        }
        public override void OnTopLayerCleared(Action<Vector2> onCleared)
        {
            onCleared?.Invoke(transform.position);
        }
        
        public override Bounds GetShelfBounds()
        {
            return sprRenderer.bounds;
        }
    }
}