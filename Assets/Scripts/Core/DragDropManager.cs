using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Central manager controls all drag & drop operations
    /// </summary>
    public class DragDropGameManager : MonoBehaviour
    {
        [SerializeField] private float dragZOffset = -1f;
        /* Object chứa các Drag & Drop */
        [SerializeField] private Transform container;

        [Header("Visual Settings")] [SerializeField]
        private bool enableVisualFeedback = true;

        [SerializeField] private Color dragColor = new(1f, 1f, 1f, 0.8f);
        [SerializeField] private Vector3 dragScale = new(1.1f, 1.1f, 1f);

        // Registered objects
        private List<DragObject> _dragObjects;
        private List<DropZone> _dropZones;

        // Current drag state
        private DragObject _currentDraggingObject;
        private Vector3 _dragOffset;
        private Camera _mainCamera;

        void Awake()
        {
            _mainCamera = Camera.main;
            if (!_mainCamera)
            {
                Debug.LogError("Main Camera not found!");
            }
            
            _dragObjects = new List<DragObject>();
            _dropZones = new List<DropZone>();
            
            // var drags = container.GetComponentsInChildren<DragObject>();
            // foreach (var c in drags)
            // {
            //     RegisterDragObject(c);
            // }
            //
            // var drops = container.GetComponentsInChildren<DropZone>();
            // foreach (var c in drops)
            // {
            //     RegisterDropZone(c);
            // }
        }

        void Update()
        {
            HandleMouseInput();
            UpdateDragging();
            UpdateDropZoneHighlights();
        }

        private void HandleMouseInput()
        {
            // Mouse down - start drag
            if (Input.GetMouseButtonDown(0) && !_currentDraggingObject)
            {
                TryStartDrag();
            }

            // Mouse up - end drag
            if (Input.GetMouseButtonUp(0) && _currentDraggingObject)
            {
                EndDrag();
            }
        }

        private void TryStartDrag()
        {
            var mouseWorldPos = GetMouseWorldPosition();
            var objectUnderMouse = GetDragObjectAtPosition(mouseWorldPos);

            if (objectUnderMouse)
            {
                StartDrag(objectUnderMouse);
            }
        }

        private void StartDrag(DragObject dragObject)
        {
            _currentDraggingObject = dragObject;

            // Calculate offset
            var mousePos = GetMouseWorldPosition();
            _dragOffset = dragObject.transform.position - mousePos;

            dragObject.OnStartDrag();

            if (enableVisualFeedback)
            {
                dragObject.ApplyDragVisuals(dragColor, dragScale);
            }
        }

        private void UpdateDragging()
        {
            if (!_currentDraggingObject) return;

            var mousePos = GetMouseWorldPosition();
            var newPosition = mousePos + _dragOffset;
            newPosition.z = _currentDraggingObject.GetOriginalPosition().z + dragZOffset;

            _currentDraggingObject.UpdatePosition(newPosition);
        }

        private void EndDrag()
        {
            if (!_currentDraggingObject) return;

            // Find drop zone at current position
            var dropPosition = _currentDraggingObject.transform.position;
            var targetZone = GetDropZoneAtPosition(dropPosition);

            var successfulDrop = false;

            if (targetZone && targetZone.CanAcceptObject(_currentDraggingObject))
            {
                var previousZone = _currentDraggingObject.GetCurrentZone();
                if (previousZone)
                {
                    previousZone.RemoveObject(_currentDraggingObject);
                }

                // Add to new zone
                targetZone.AddObject(_currentDraggingObject);
                _currentDraggingObject.SetCurrentZone(targetZone);
                
                // replace parent
                _currentDraggingObject.transform.SetParent(targetZone.transform, true);

                // Snap position if needed
                if (targetZone.ShouldSnapToCenter())
                {
                    _currentDraggingObject.UpdatePosition(targetZone.GetSnapPosition(0));
                }
                else
                {
                    // Get Local position of drop position inside targetZone
                    var localPosition = targetZone.transform.InverseTransformPoint(dropPosition);
                    var slot = targetZone.GetSlotAtPosition(localPosition);
                    var newPosition = targetZone.GetSnapPosition(slot);
                    _currentDraggingObject.UpdatePosition(newPosition);
                }

                successfulDrop = true;
            }

            // Handle unsuccessful drop
            if (!successfulDrop && _currentDraggingObject.ShouldReturnToOriginal())
            {
                _currentDraggingObject.ReturnToOriginalPosition();
            }

            _currentDraggingObject.ResetVisuals();
            _currentDraggingObject.OnEndDrag();
            _currentDraggingObject = null;
        }

        private void UpdateDropZoneHighlights()
        {
            if (!_currentDraggingObject)
            {
                // Reset all highlights when not dragging
                foreach (var zone in _dropZones)
                {
                    zone.SetHighlight(false, false);
                }

                return;
            }

            // Update highlights based on current drag position
            var dragPos = _currentDraggingObject.transform.position;

            foreach (var zone in _dropZones)
            {
                var isOver = zone.ContainsPosition(dragPos);
                var canAccept = zone.CanAcceptObject(_currentDraggingObject);
                zone.SetHighlight(isOver, canAccept);
            }
        }

        // Helper methods
        private Vector3 GetMouseWorldPosition()
        {
            var mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            return mousePos;
        }
        
        private DragObject GetDragObjectAtPosition(Vector2 position)
        {
            foreach (var dragObj in _dragObjects)
            {
                if (dragObj.ContainsPosition(position) && dragObj.CanBeDragged())
                {
                    return dragObj;
                }
            }
            return null;
        }

        private DropZone GetDropZoneAtPosition(Vector2 position)
        {
            // Check zones in reverse order (top to bottom)
            for (var i = _dropZones.Count - 1; i >= 0; i--)
            {
                if (_dropZones[i].ContainsPosition(position) && _dropZones[i].IsActive())
                {
                    return _dropZones[i];
                }
            }
            
            return null;
        }
        
        // Public registration methods
        public void RegisterDragObject(DragObject dragObject)
        {
            if (!_dragObjects.Contains(dragObject))
            {
                _dragObjects.Add(dragObject);
            }
        }

        public void UnregisterDragObject(DragObject dragObject)
        {
            _dragObjects.Remove(dragObject);
        }

        public void RegisterDropZone(DropZone dropZone)
        {
            if (!_dropZones.Contains(dropZone))
            {
                _dropZones.Add(dropZone);
                // Sort by sorting order for proper overlap detection
                _dropZones.Sort((a, b) => a.GetSortingOrder().CompareTo(b.GetSortingOrder()));
            }
        }

        public void UnregisterDropZone(DropZone dropZone)
        {
            _dropZones.Remove(dropZone);
        }

        // Public utility methods
        public bool IsDragging()
        {
            return _currentDraggingObject != null;
        }

        public DragObject GetCurrentDraggingObject()
        {
            return _currentDraggingObject;
        }

        public void ResetAllObjects()
        {
            foreach (var obj in _dragObjects)
            {
                obj.ReturnToOriginalPosition();
            }

            foreach (var zone in _dropZones)
            {
                zone.ClearAllObjects();
            }
        }
    }
}