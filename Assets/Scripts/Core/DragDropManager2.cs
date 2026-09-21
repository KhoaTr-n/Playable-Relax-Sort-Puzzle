using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Central manager controls all drag & drop operations
    /// </summary>
    public class DragDropManager2 : MonoBehaviour, IDragDropManager
    {
        [SerializeField] private float dragZOffset = -1f;

        /* Object chứa các Drag & Drop */
        [SerializeField] private Transform container;

        // State
        private bool _hasInteracted;
        
        [Header("Drop Target Highlight")] //
        [SerializeField]
        private bool enableVisualFeedback = true;

        [SerializeField] private Color dragColor = new Color(1f, 1f, 1f, 0.8f);
        [SerializeField] private Vector3 dragScale = new Vector3(1.1f, 1.1f, 1f);

        private Func<IDropZone, bool> _canAcceptDropIntoFunc;
        private Action<ShelfItemMeta> _onItemPicked;

        // Registered objects
        private List<IDragObject> _dragObjects;
        private List<DropZoneData> _dropZones;

        // Current drag state
        private IDragObject _currentDraggingObject;
        private Vector3 _dragOffset;
        private Camera _mainCamera;
        private bool _pause;

        private void Awake()
        {
            _mainCamera = Camera.main;
            if (!_mainCamera)
            {
                Debug.LogError("Main Camera not found!");
            }

            _dragObjects = new List<IDragObject>();
            _dropZones = new List<DropZoneData>();
        }

        private void Update()
        {
            if (_pause) return;
            HandleMouseInput();
            UpdateDragging();
        }

        #region PUBLIC_METHODS

        public void Init(Func<IDropZone, bool> canAcceptDropIntoFunc)
        {
            _canAcceptDropIntoFunc = canAcceptDropIntoFunc;
        }

        // Public registration methods
        public void RegisterDragObject(IDragObject dragObject)
        {
            if (dragObject == null) return;
            if (!_dragObjects.Contains(dragObject))
            {
                _dragObjects.Add(dragObject);
            }
        }

        public void UnregisterDragObject(int dragId)
        {
            var drag = _dragObjects.FirstOrDefault(d => d.Id == dragId);
            if (drag == null) return;
            _dragObjects.Remove(drag);
            if (_currentDraggingObject?.Id == dragId) _currentDraggingObject = null;
        }

        public void RegisterDropZone(DropZoneData dropZone)
        {
            UnregisterDropZone(dropZone.Zone);
            _dropZones.Add(dropZone);
        }

        public void UnregisterDropZone(IDropZone dropZone)
        {
            var index = _dropZones.FindIndex(e => e.Zone == dropZone);
            if (index >= 0)
            {
                _dropZones.RemoveAt(index);
            }
        }

        public void RegisterItemPickedAction(Action<ShelfItemMeta> onPicked)
        {
            _onItemPicked = onPicked;
        } 

        // Public utility methods
        public bool IsDragging()
        {
            return _currentDraggingObject != null;
        }

        public IDragObject GetCurrentDraggingObject()
        {
            return _currentDraggingObject;
        }

        public void ResetAllObjects()
        {
            foreach (var obj in _dragObjects)
            {
                obj.ReturnToOriginalPosition();
            }
        }

        public void RemoveAll()
        {
            _currentDraggingObject = null;
            _dragObjects.Clear();
            _dropZones.Clear();
        }

        public void Pause()
        {
            _pause = true;
        }

        public void ReleaseDraggingObject()
        {
            if (_currentDraggingObject == null) return;
            if (_currentDraggingObject is MonoBehaviour mb && mb == null)
            {
                _currentDraggingObject = null;
                return;
            }

            _currentDraggingObject.ReturnToOriginalPosition();
            _currentDraggingObject.ResetVisuals();
            _currentDraggingObject.OnEndDrag();
            _currentDraggingObject = null;
        }

        public void Unpause()
        {
            _pause = false;
        }

        public void ManualDropInto(IDragObject dragObject, IDropZone dropZone)
        {
            var targetZoneData = _dropZones.Find(e => e.Zone == dropZone);
            DropInto(dragObject, targetZoneData);
        }

        #endregion

        #region PRIVATE_METHODS

        private void HandleMouseInput()
        {
            // Mouse down - start drag
            if (Input.GetMouseButtonDown(0) && _currentDraggingObject == null)
            {
                TryStartDrag();
            }

            // Mouse up - end drag
            if (Input.GetMouseButtonUp(0) && _currentDraggingObject != null)
            {
                EndDrag();
            }
        }

        private void TryStartDrag()
        {
            var mouseWorldPos = GetMouseWorldPosition();
            var objectUnderMouse = GetDragObjectAtPosition(mouseWorldPos);

            if (objectUnderMouse != null)
            {
                StartDrag(objectUnderMouse);
            }
        }

        private void StartDrag(IDragObject dragObject)
        {
            if (!_hasInteracted)
            {
                _hasInteracted = true;
            }

            _currentDraggingObject = dragObject;

            // Calculate offset
            _dragOffset = new Vector3(0, -0.5f, 0);

            dragObject.OnStartDrag();

            if (enableVisualFeedback)
            {
                dragObject.ApplyDragVisuals(dragColor, dragScale);
            }
        }

        private void UpdateDragging()
        {
            if (_currentDraggingObject == null) return;

            var mousePos = GetMouseWorldPosition();
            var newPosition = mousePos + _dragOffset;
            newPosition.z = _currentDraggingObject.GetOriginalPosition().z + dragZOffset;

            _currentDraggingObject.UpdatePosition(newPosition);
        }

        private void EndDrag()
        {
            /***
             * Trong một frame nào đó gameObject thuộc về _currentDraggingObject đã bị destroy.
             * Nhưng null check _currentDraggingObject == null trả về false bởi vì Unity nạp chồng toán tử so sánh bằng == để so sánh C# reference và không
             * sử dụng Unity null check, lúc này chưa bị GC dọn dẹp
             */
            if (_currentDraggingObject == null) return;
            if (_currentDraggingObject is MonoBehaviour mb && mb == null)
            {
                _currentDraggingObject = null;
                return;
            }
            var dragObject = _currentDraggingObject;

            var dropPosition = dragObject.Position;
            var originalPosition = dragObject.GetOriginalPosition();

            var dragDistance = Vector2.Distance(
                new Vector2(dropPosition.x, dropPosition.y),
                new Vector2(originalPosition.x, originalPosition.y)
            );

            const float minDragDistance = 0.33f;

            var successfulDrop = false;

            if (dragDistance >= minDragDistance)
            {
                var targetZoneData = GetDropZoneAtPosition(dropPosition);
                if (targetZoneData != null)
                {
                    successfulDrop = DropInto(dragObject, targetZoneData);
                }
            }

            if (!successfulDrop && dragObject.ShouldReturnToOriginal())
            {
                dragObject.ReturnToOriginalPosition();
            }

            dragObject.ResetVisuals();
            dragObject.OnEndDrag();
            _currentDraggingObject = null;
        }

        // Helper methods
        private Vector3 GetMouseWorldPosition()
        {
            var mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            return mousePos;
        }

        private IDragObject GetDragObjectAtPosition(Vector2 position)
        {
            foreach (var dragObj in _dragObjects)
            {
                if (dragObj.CanBeDragged() && dragObj.ContainsPosition(position))
                {
                    return dragObj;
                }
            }

            return null;
        }

        [CanBeNull]
        private DropZoneData GetDropZoneAtPosition(Vector2 position)
        {
            var dragObjectBounds = _currentDraggingObject.GetSpriteBounds();

            var shelfOverlaps = new Dictionary<int, (DropZone2 zone, float overlap)>();

            foreach (var zoneData in _dropZones)
            {
                var zone2 = zoneData.Zone as DropZone2;
                if (zone2 == null) continue;

                var shelfId = zone2.ShelfId;

                if (shelfOverlaps.ContainsKey(shelfId)) continue;

                var shelfOverlap = zone2.GetShelfOverlapArea(dragObjectBounds);
                if (shelfOverlap > 0)
                {
                    shelfOverlaps[shelfId] = (zone2, shelfOverlap);
                }
            }

            if (shelfOverlaps.Count == 0) return null;

            var bestShelf = shelfOverlaps.OrderByDescending(e => e.Value.overlap).First();
            var bestZone2 = bestShelf.Value.zone;

            var emptyDropZone = bestZone2.GetNearestEmptyDropZone(position);
            if (emptyDropZone == null) return null;

            return _dropZones.Find(e => e.Zone == emptyDropZone);
        }

        private bool DropInto(IDragObject dragObject, DropZoneData targetZoneData)
        {
            var targetZone = targetZoneData.Zone;

            if (targetZone == null || !_canAcceptDropIntoFunc(targetZone)) return false;

            // Snap position if needed
            if (targetZone.ShouldSnapToCenter())
            {
                dragObject.UpdatePosition(targetZone.GetSnapPosition(0));
            }

            StartCoroutine(ScheduleCallback(targetZoneData, dragObject.Id));
            return true;
        }

        #endregion

        private static IEnumerator ScheduleCallback(DropZoneData dropZone, int dragId)
        {
            yield return null; // next frame
            dropZone.OnDropped(dragId);
        }

    }

    public class DropZoneData
    {
        public IDropZone Zone;
        public Action<int> OnDropped;
    }
}