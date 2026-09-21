using System;
using System.Collections.Generic;
using Core;
using Engine.ShelfPuzzle;
using Game;
using Luna.Unity;
using Strategy.Level;
using UnityEngine;
using UnityEngine.U2D;
using DG.Tweening;
using Random = UnityEngine.Random;

namespace PlayableAds
{
    [Serializable]
    public class PlayableAdLevelDef
    {
        public PlayableAdShelfDef[] shelves;
    }

    [Serializable]
    public class PlayableAdShelfDef
    {
        public int[] slots;
    }

    /// <summary>
    /// MonoBehaviour gắn vào scene để test Playable Ads.
    /// Shelves kéo vào Inspector theo columns (bottom → top).
    /// Items tự spawn từ SpriteAtlas, shuffle ngẫu nhiên, đảm bảo không có shelf nào bắt đầu với 3 items giống nhau.
    /// </summary>
    public class PlayableAdsLevelSetup : MonoBehaviour
    {
        [Header("Grid Settings")]
        [LunaPlaygroundField("CTA Threshold", 1, "Level Settings")]
        public int ctaThreshold = 5;
        
        [SerializeField] private int columnCount = 5;
        [SerializeField] private int rowCount = 4;

        [Header("Prefabs & Assets")]
        [SerializeField] private CommonShelfNormal shelfPrefab;
        [SerializeField] private ShelfItemBasic itemPrefab;
        [SerializeField] private SpriteAtlas spriteAtlas;

        [Header("Drag Drop")]
        [SerializeField] private DragDropManager2 dragDropManager;

        [Header("Số slot trống")]
        [SerializeField] private int emptySlot;

        [Header("Fixed Level Data (Tùy chọn)")]
        [SerializeField] private TextAsset levelJsonFile;

        [Header("Tutorial")]
        [SerializeField] private GameObject tutorialHandPrefab;

        private GameObject _tutorialHand;
        private Sequence _tutorialSequence;
        private PlayableAdsLevelAnimation _levelAnimation;
        private ILevelDataManager _levelDataManager;
        private Dictionary<int, Sprite> _spriteMap; // typeId → Sprite

        private const float TargetTotalWidth = 10.68f;

        private float StartY
        {
            get
            {
                var mainCam = Camera.main;
                var orthoSize = mainCam != null ? mainCam.orthographicSize : 9.13f;
                return -orthoSize + 1.35f;
            }
        }
        private const float DeltaX = 3.38f;
        private const float DeltaY = 2.17f;
        private const float DeltaZ = -0.3f;

        /// <summary>
        /// Tính và set orthographicSize cho Camera.main theo công thức:
        /// orthographicSize = TargetTotalWidth / (2 * aspectRatio)
        /// </summary>
        private void AdjustCameraOrthographicSize()
        {
            var mainCam = Camera.main;
            if (mainCam == null) return;

            var aspect = mainCam.aspect;
            if (aspect <= 0f)
            {
                aspect = (float)Screen.width / Screen.height;
            }

            mainCam.orthographicSize = TargetTotalWidth / (2f * aspect);
            Debug.Log($"[PlayableAds] Set orthographicSize = {mainCam.orthographicSize:F2} (aspect = {aspect:F4})");
        }

        private void Awake()
        {
            // Tắt multi-touch (chống nhấn nhiều ngón tay cùng lúc)
            Input.multiTouchEnabled = false;

            // Thiết lập FPS 60 và tắt vSync để targetFrameRate có hiệu lực
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            // 0. Set camera orthographic size trước tiên
            AdjustCameraOrthographicSize();

            if (shelfPrefab == null)
            {
                Debug.LogError("ShelfPrefab chưa được gán");
                return;
            }

            // 1. Load sprites từ atlas
            LoadSprites();
            if (_spriteMap.Count == 0)
            {
                Debug.LogError("SpriteAtlas rỗng");
                return;
            }

            var totalShelves = columnCount * rowCount;
            const int slotsPerShelf = 3;

            // 2. Generate or Load item distribution
            List<int> distribution;
            if (levelJsonFile != null)
            {
                distribution = LoadDistributionFromJson();
                // Override total shelves based on JSON to avoid out-of-bounds
                totalShelves = distribution.Count / slotsPerShelf;
                // rowCount = totalShelves / columnCount
                rowCount = totalShelves / columnCount;
            }
            else
            {
                distribution = GenerateDistribution(totalShelves, slotsPerShelf);
            }

            // 3. Generate grid of shelves
            var generatedShelves = GenerateShelfGrid();

            // 4. Build grid + init shelves + spawn items
            var gridData = new PlayableAdsGridData(columnCount, rowCount);
            var allShelves = new IShelf2[totalShelves];
            var allShelvesItems = new IShelfItem[totalShelves][][];
            var itemId = 0;

            for (var col = 0; col < columnCount; col++)
            {
                for (var row = 0; row < rowCount; row++)
                {
                    var shelfIndex = col * rowCount + row;
                    var shelf = generatedShelves[col, row];

                    shelf.Init(shelfIndex);
                    gridData.RegisterShelf(shelfIndex, col, row, shelf.transform.position);
                    allShelves[shelfIndex] = shelf;

                    // Spawn items — 1 layer, 3 slots
                    allShelvesItems[shelfIndex] = new IShelfItem[1][];
                    allShelvesItems[shelfIndex][0] = new IShelfItem[slotsPerShelf];

                    for (var slotId = 0; slotId < slotsPerShelf; slotId++)
                    {
                        var typeId = distribution[shelfIndex * slotsPerShelf + slotId];
                        if (typeId == 0)
                        {
                            // Slot trống
                            allShelvesItems[shelfIndex][0][slotId] = null;
                            continue;
                        }

                        var meta = new ShelfItemMeta(shelfIndex, 0, slotId, typeId, itemId++);
                        var sprite = _spriteMap[typeId];

                        var item = Instantiate(itemPrefab, shelf.transform);
                        item.Init(
                            meta,
                            shelf.SpacingData,
                            ShelfLayerDisplay.Top,
                            OnItemDestroy,
                            GetSprite,
                            sprite,
                            shelf
                        );
                        item.ResetVisual();

                        allShelvesItems[shelfIndex][0][slotId] = item;
                    }
                }
            }

            // 5. Create managers
            var levelData = new LevelData(allShelvesItems, allShelves);
            _levelDataManager = new LevelDataManager(levelData);

            // Init drag drop
            dragDropManager.Init(CanAcceptDropInto);

            // 6. Create animation
            _levelAnimation = new PlayableAdsLevelAnimation(
                _levelDataManager,
                dragDropManager,
                gridData,
                ctaThreshold
            );
            _levelAnimation.Enter();

            DOVirtual.DelayedCall(1f, ShowTutorial);
        }

        private void ShowTutorial()
        {
            if (tutorialHandPrefab == null || _levelDataManager == null) return;

            var startShelf = _levelDataManager.GetShelf(0);
            var endShelf = _levelDataManager.GetShelf(16);

            if (startShelf == null || endShelf == null) return;

            var startZone = startShelf.DropZones[0];
            var endZone = endShelf.DropZones[2];

            var startPos = startZone.GetSnapPosition(0);
            var endPos = endZone.GetSnapPosition(0);

            // Z offset for hand so it renders on top
            startPos.z = -2f;
            endPos.z = -2f;

            var parentTransform = startShelf.UnityTransform as Transform;
            _tutorialHand = Instantiate(tutorialHandPrefab, startPos, Quaternion.identity, parentTransform);

            Analytics.LogEvent(Analytics.EventType.TutorialStarted, 0);

            _tutorialSequence = DOTween.Sequence();
            _tutorialSequence.Append(_tutorialHand.transform.DOMove(endPos, 1f).SetEase(Ease.InOutSine))
                .AppendInterval(0.2f)
                .AppendCallback(() => _tutorialHand.transform.position = startPos)
                .AppendInterval(0.2f)
                .SetLoops(-1, LoopType.Restart);
        }

        /// <summary>
        /// Tự generate lưới shelf từ prefab.
        /// Sắp xếp bottom → top, align giữa theo trục X.
        /// Số lẻ columns: shelf giữa ở x=0.
        /// Số chẵn columns: hai shelf giữa đối xứng quanh x=0.
        /// </summary>
        private CommonShelfNormal[,] GenerateShelfGrid()
        {
            var shelves = new CommonShelfNormal[columnCount, rowCount];

            // Tính offset X để align giữa
            // Với n columns, tổng chiều rộng = (n - 1) * DeltaX
            // X bắt đầu = -(n - 1) * DeltaX / 2
            var startX = -(columnCount - 1) * DeltaX / 2f;

            for (var col = 0; col < columnCount; col++)
            {
                var x = startX + col * DeltaX;
                for (var row = 0; row < rowCount; row++)
                {
                    var y = StartY + row * DeltaY;
                    var z = row * DeltaZ;
                    var position = new Vector3(x, y, z);

                    var shelf = Instantiate(shelfPrefab, position, Quaternion.identity, transform);
                    shelf.name = $"Shelf_C{col}_R{row}";
                    shelves[col, row] = shelf;
                }
            }

            return shelves;
        }

        private void Update()
        {
            _levelAnimation?.Update(Time.deltaTime);

            if (_tutorialHand != null && Input.GetMouseButtonDown(0))
            {
                _tutorialSequence?.Kill();
                Destroy(_tutorialHand);
                _tutorialHand = null;

                Analytics.LogEvent(Analytics.EventType.TutorialComplete, 0);
            }
        }

        private void OnDestroy()
        {
            _tutorialSequence?.Kill();
            _levelAnimation?.Dispose();
        }

        // ======== Drop acceptance ========

        private bool CanAcceptDropInto(IDropZone dropzone)
        {
            if (_levelDataManager == null) return false;

            var shelf = _levelDataManager.GetShelf(dropzone.ShelfId);
            if (shelf == null) return false;

            var layer = _levelDataManager.GetTopLayer(dropzone.ShelfId);
            if (layer == null) return false;
            if (layer.Length == 0) return true;
            if (dropzone.SlotId < 0 || dropzone.SlotId >= layer.Length) return false;

            return layer[dropzone.SlotId] == null;
        }

        // ======== Sprite loading ========

        private void LoadSprites()
        {
            var count = spriteAtlas.spriteCount;
            var sprites = new Sprite[count];
            spriteAtlas.GetSprites(sprites);

            _spriteMap = new Dictionary<int, Sprite>();
            for (var i = 0; i < count; i++)
            {
                _spriteMap[i + 1] = sprites[i]; // typeId bắt đầu từ 1 (0 = null/empty)
            }
        }

        private Sprite GetSprite(ShelfItemMeta meta)
        {
            return _spriteMap.TryGetValue(meta.TypeId, out var spr) ? spr : null;
        }

        private void OnItemDestroy(ShelfItemMeta meta)
        {
            dragDropManager.UnregisterDragObject(meta.Id);
        }

        // ======== Item distribution ========

        private List<int> LoadDistributionFromJson()
        {
            var levelDef = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayableAdLevelDef>(levelJsonFile.text);
            var items = new List<int>();
            foreach (var shelf in levelDef.shelves)
            {
                items.AddRange(shelf.slots);
            }
            return items;
        }

        /// <summary>
        /// Sinh danh sách typeIds cho tất cả slots.
        /// Mỗi type xuất hiện đúng 3 lần (để merge được).
        /// Để lại ít nhất 6 slot trống (typeId = 0).
        /// Đảm bảo không shelf nào bắt đầu với 3 items giống nhau.
        /// </summary>
        private List<int> GenerateDistribution(int totalShelves, int slotsPerShelf)
        {
            int emptySlots = emptySlot;
            var totalSlots = totalShelves * slotsPerShelf;
            var numberOfTypes = _spriteMap.Count;

            // Số bộ items = (totalSlots - emptySlots) / 3
            var groupsNeeded = (totalSlots - emptySlots) / slotsPerShelf;

            // Tạo list: mỗi group 3 items cùng type, cycle qua các type
            var items = new List<int>(totalSlots);
            for (var g = 0; g < groupsNeeded; g++)
            {
                var typeId = (g % numberOfTypes) + 1;
                for (var s = 0; s < slotsPerShelf; s++)
                {
                    items.Add(typeId);
                }
            }

            // Pad slots còn lại bằng 0 (empty)
            while (items.Count < totalSlots)
            {
                items.Add(0);
            }

            // Fisher-Yates shuffle
            for (var i = items.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }

            // Fix shelf nào có 3 items giống nhau (chỉ check items thực, bỏ qua 0)
            FixThreeInARow(items, slotsPerShelf);

            return items;
        }

        /// Swap items để phá vỡ shelf có 3 items giống nhau.
        private static void FixThreeInARow(List<int> items, int slotsPerShelf)
        {
            var totalShelves = items.Count / slotsPerShelf;

            for (var s = 0; s < totalShelves; s++)
            {
                var baseIdx = s * slotsPerShelf;
                if (items[baseIdx] != items[baseIdx + 1] || items[baseIdx + 1] != items[baseIdx + 2])
                    continue;

                // Shelf s có 3 items giống nhau → swap items[baseIdx + 2]
                for (var other = 0; other < items.Count; other++)
                {
                    if (other / slotsPerShelf == s) continue;
                    if (items[other] == items[baseIdx]) continue;

                    var otherBase = (other / slotsPerShelf) * slotsPerShelf;

                    // Swap tạm
                    (items[baseIdx + 2], items[other]) = (items[other], items[baseIdx + 2]);

                    // Kiểm tra cả 2 shelf
                    var shelfOk = !(items[baseIdx] == items[baseIdx + 1] &&
                                    items[baseIdx + 1] == items[baseIdx + 2]);
                    var otherOk = !(items[otherBase] == items[otherBase + 1] &&
                                    items[otherBase + 1] == items[otherBase + 2]);

                    if (shelfOk && otherOk)
                        break;

                    // Revert
                    (items[baseIdx + 2], items[other]) = (items[other], items[baseIdx + 2]);
                }
            }
        }
    }
}
