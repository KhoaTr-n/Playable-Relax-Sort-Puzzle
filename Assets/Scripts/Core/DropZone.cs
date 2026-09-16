using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

namespace Core
{
    public class DropZone : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Zone Settings")] [SerializeField]
        private bool isActive = true;

        [SerializeField] private int maxObjects = 1;
        [SerializeField] private string[] acceptedTypes;
        [SerializeField] private bool snapToCenter = true;
        // [SerializeField] private Vector3 snapOffset = Vector3.zero;

        [Header("Visual Settings")] [SerializeField]
        private Color normalColor = new(1, 1, 1, 0.3f);

        [SerializeField] private Color hoverValidColor = new(0, 1, 0, 0.5f);
        [SerializeField] private Color hoverInvalidColor = new(1, 0, 0, 0.5f);
        [SerializeField] private bool showDebugBounds = true;

        private List<DragObject> _containedObjects;
        private bool _isHighlighted = false;
        private bool _canAcceptCurrent = false;

        private void Start()
        {
            _containedObjects = new List<DragObject>();
        }

        // Called by manager
        public bool ContainsPosition(Vector2 position)
        {
            if (!isActive) return false;
            return spriteRenderer.bounds.Contains(position);
        }

        public bool CanAcceptObject(DragObject dragObject)
        {
            if (!isActive) return false;
            if (_containedObjects.Count >= maxObjects) return false;

            // Check type restrictions
            if (acceptedTypes != null && acceptedTypes.Length > 0)
            {
                var objType = dragObject.GetObjectType();
                var typeAccepted = false;

                foreach (var acceptedType in acceptedTypes)
                {
                    if (objType == acceptedType)
                    {
                        typeAccepted = true;
                        break;
                    }
                }

                if (!typeAccepted) return false;
            }

            return true;
        }
        
        public bool CanDrop(DragObject dragObject)
        {
            // Thả được tại slot i không, không được tìm slot kế bên
            return true;
        }

        public void AddObject(DragObject dragObject)
        {
            if (!_containedObjects.Contains(dragObject))
            {
                _containedObjects.Add(dragObject);
            }
        }

        public void RemoveObject(DragObject dragObject)
        {
            _containedObjects.Remove(dragObject);
        }

        public void ClearAllObjects()
        {
            _containedObjects.Clear();
        }
        
        public void SetHighlight(bool highlighted, bool canAccept)
        {
            _isHighlighted = highlighted;
            _canAcceptCurrent = canAccept;

            if (spriteRenderer)
            {
                if (!highlighted)
                {
                    spriteRenderer.color = normalColor;
                }
                else
                {
                    spriteRenderer.color = canAccept ? hoverValidColor : hoverInvalidColor;
                }
            }
        }

        // Getters
        public Vector3 GetSnapPosition(int slot)
        {
            var offset = new Vector3(slot, 0, 0);
            return transform.position + offset;
        }
        
        /// <summary>
        /// Để cho người chơi thả vào không cần chính xác vị trí drop, item vẫn drop đến vị trí gần nhất.
        /// Kể cả chỗ đó đã có item khác giữ vị trí rồi => drop vị trí kế bên
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public int GetSlotAtPosition(Vector2 position)
        {
            if (position.x < -0.5f)
                return -1;

            if (position.x > 0.5f)
                return 1;

            return 0;
        }

        public bool ShouldSnapToCenter() => snapToCenter;
        public int GetSortingOrder() => spriteRenderer ? spriteRenderer.sortingOrder : 0;
        public List<DragObject> GetContainedObjects() => new(_containedObjects);
        public int GetObjectCount() => _containedObjects.Count;
        public bool IsActive() => isActive;
        public void SetActive(bool active) => isActive = active;

        // Helper to create default sprite
        private Sprite CreateSquareSprite()
        {
            var texture = new Texture2D(100, 100);
            var pixels = new Color[100 * 100];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, 100, 100), new Vector2(0.5f, 0.5f), 100);
        }

        // Debug visualization
        void OnDrawGizmos()
        {
            if (!showDebugBounds) return;

            Gizmos.color = _isHighlighted ? (_canAcceptCurrent ? hoverValidColor : hoverInvalidColor) : normalColor;

            if (spriteRenderer != null)
            {
                var bounds = spriteRenderer.bounds;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
            else
            {
                Gizmos.DrawWireCube(transform.position, Vector3.one);
            }

            // Draw snap position
            if (snapToCenter)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(GetSnapPosition(0), 0.1f);
            }
        }
    }
}