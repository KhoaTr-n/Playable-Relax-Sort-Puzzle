using System;
using UnityEngine;

namespace Core
{
    public class DragObject2 : MonoBehaviour, IDragObject
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Settings")] // 
        [SerializeField]
        private bool isDraggable = true;

        [SerializeField] private bool returnToOriginal = true;
        [SerializeField] private bool useCustomBounds = true;
        [SerializeField] private Vector2 customBounds = new(1f, 1.8f);

        public int Id { get; private set; }
        public Vector3 Position => transform.position;

        // State
        private Vector3 _originalPosition;
        private Vector3 _originalLocalPosition;
        private Transform _originalParent;
        private Color _originalColor;
        private Vector3 _originalScale;
        private int _originalSortingOrder;
        private bool _isBeingDragged = false;
        private Func<bool> _canBeDragged;

        private void Start()
        {
            // Store original values
            _originalPosition = transform.position;
            _originalColor = Color.white;
            _originalScale = transform.localScale;
            _originalSortingOrder = spriteRenderer.sortingOrder;
        }

        public void Init(int id, Func<bool> canBeDragged)
        {
            Id = id;
            _canBeDragged = canBeDragged;
        }

        // Called by manager
        public bool ContainsPosition(Vector2 position)
        {
            var objectBound = spriteRenderer.bounds;
            var center = objectBound.center;
            
            if(useCustomBounds)
            {
                var foot = spriteRenderer.transform.position;
                center = new Vector3(foot.x, foot.y + customBounds.y / 2f, foot.z);
                var size = new Vector3(customBounds.x, customBounds.y, 0f);
                objectBound = new Bounds(center, size);
            }
            else
            {
                objectBound = spriteRenderer.bounds;
            }

            var testPos = new Vector3(position.x, position.y, center.z);
            return objectBound.Contains(testPos);
        }

        public bool CanBeDragged()
        {
            return isDraggable && !_isBeingDragged && isActiveAndEnabled && _canBeDragged();
        }

        public void OnStartDrag()
        {
            _isBeingDragged = true;
            _originalParent = transform.parent;
            _originalLocalPosition = transform.localPosition;
            _originalPosition = transform.position;
            transform.SetParent(null, true);
            // Bring to front
            spriteRenderer.sortingOrder = _originalSortingOrder + 100;
        }

        public void OnEndDrag()
        {
            _isBeingDragged = false;
            spriteRenderer.sortingOrder = _originalSortingOrder;
        }

        public void UpdatePosition(Vector3 newPosition)
        {
            if (this != null && transform != null)
            {
                transform.position = newPosition;
            }
        }

        public void ApplyDragVisuals(Color dragColor, Vector3 dragScale)
        {
            spriteRenderer.color = dragColor;
            transform.localScale = dragScale;
        }

        public void ResetVisuals()
        {
            spriteRenderer.color = _originalColor;
            transform.localScale = _originalScale;
        }

        public void ReturnToOriginalPosition()
        {
            if (_originalParent != null)
            {
                transform.SetParent(_originalParent, false);
                transform.localPosition = _originalLocalPosition;
            }
            else
            {
                transform.position = _originalPosition;
            }
        }

        // 🔍 Debug Gizmo: visualize Sprite bounds
        private void OnDrawGizmosSelected()
        {
            if (spriteRenderer == null)
                return;
            
            var foot = spriteRenderer.transform.position;
            var center = new Vector3(foot.x, foot.y + 0.9f, foot.z);
            var size = new Vector3(1f, 1.8f, 0f);           
            var expandedBounds = new Bounds(center, size);

            Gizmos.color = Color.blue;

            Gizmos.DrawWireCube(expandedBounds.center, expandedBounds.size);
        }

        // Getters/Setters
        public Vector3 GetOriginalPosition() => _originalPosition;
        public Bounds GetSpriteBounds() => spriteRenderer.bounds;
        public bool ShouldReturnToOriginal() => returnToOriginal;
        public void SetDraggable(bool draggable) => isDraggable = draggable;
    }
}