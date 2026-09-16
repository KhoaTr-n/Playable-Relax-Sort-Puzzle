using System;
using System.Collections.Generic;
using Core;
using DG.Tweening;
using UnityEngine;

namespace Game
{
    public class ShelfItemBasic : MonoBehaviour, IShelfItem
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] public DragObject2 dragObject;

        public ShelfItemMeta Meta { get; private set; }
        public ShelfLayerDisplay CurrentDisplay { get; private set; }
        public IDragObject DragObject => dragObject;
        public object UnitySpriteRenderer => spriteRenderer;

        private const float MaxZ = -1.0f;
        private ISpacingData _spacingData;
        private Action<ShelfItemMeta> _onDestroy;
        private Func<ShelfItemMeta, Sprite> _getSprite;
        private IShelf2 _shelf;

        private List<Tween> _runningTween;

        private void Awake()
        {
#if !UNITY_EDITOR
            Destroy(debugText.gameObject);
#endif
            _runningTween = new List<Tween>();
        }

        public void Init(
            ShelfItemMeta meta,
            ISpacingData spacingData,
            ShelfLayerDisplay display,
            Action<ShelfItemMeta> onDestroy,
            Func<ShelfItemMeta, Sprite> getSprite,
            Sprite sprite,
            IShelf2 shelf
        )
        {
            Meta = meta;
            _spacingData = spacingData;
            CurrentDisplay = display;
            _onDestroy = onDestroy;
            _getSprite = getSprite;
            _shelf = shelf;
            spriteRenderer.sprite = sprite;
            name = $"S{meta.ShelfId}-L{meta.LayerId}-T{meta.TypeId}-{meta.Id}";
            dragObject.Init(meta.Id, CanBeDragged);
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void SetShelf(
            ShelfItemMeta meta,
            ISpacingData spacingData,
            ShelfLayerDisplay display
        )
        {
            Meta = meta;
            _spacingData = spacingData;
            CurrentDisplay = display;
        }

        public void SetNewMeta(ShelfItemMeta newMeta)
        {
            Meta = newMeta;
            spriteRenderer.sprite = _getSprite(newMeta);
        }

        public void SetDisplay(ShelfLayerDisplay display)
        {
            CurrentDisplay = display;
        }

        private bool CanBeDragged()
        {
            // Check display layer
            if (CurrentDisplay != ShelfLayerDisplay.Top) return false;

            return true;
        }

        public void ResetVisual()
        {
            if (CurrentDisplay == ShelfLayerDisplay.Hidden)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            // Position
            var layerOrder = (int)CurrentDisplay;
            var offset = (Vector3)_spacingData.GetPosition(layerOrder, Meta.SlotId);
            offset.z = MaxZ + layerOrder * 0.01f; // z càng nhỏ thì càng hiển thị trên cùng
            if (Meta.SlotId == 1)
            {
                offset.z -= 0.001f; // cho item ở giữa hiển thị nổi bật hơn
            }

            transform.localPosition = offset;
            spriteRenderer.color = GetDisplayColor(CurrentDisplay);
            transform.localScale = new Vector3(1f, 1f, 1f);
        }

        public void FadeInVisual(float duration)
        {
            if (CurrentDisplay == ShelfLayerDisplay.Top)
            {
                gameObject.SetActive(true);
                var layerOrder = (int)CurrentDisplay;
                var offset = (Vector3)_spacingData.GetPosition(layerOrder, Meta.SlotId);
                offset.z = MaxZ + layerOrder * 0.01f; // z càng nhỏ thì càng hiển thị trên cùng
                if (Meta.SlotId == 1)
                {
                    offset.z -= 0.001f; // cho item ở giữa hiển thị nổi bật hơn 
                }

                // transform.localPosition = offset;
                var targetColor = GetDisplayColor(CurrentDisplay);
                _runningTween.Add(spriteRenderer.DOColor(targetColor, duration).SetEase(Ease.OutQuad));
                _runningTween.Add(transform.DOLocalMove(offset, 0.2f).SetEase(Ease.OutQuad));
            }
        }

        public void DestroyItem(bool playEffect)
        {
            if (playEffect)
            {
                var effectPosition = new Vector3(
                    transform.position.x,
                    transform.position.y + 0.5f,
                    transform.position.z
                );
                // EffectUtils.Blink(effectPosition);
            }

            foreach (var t in _runningTween)
            {
                t.Kill();
            }

            _runningTween.Clear();

            _onDestroy.Invoke(Meta);
            Destroy(gameObject);
        }

        public void Bounce(Action onCompleted, float delay = 0f)
        {
            // Reset scale
            transform.localScale = Vector3.one;

            // Sequence tween
            var seq = DOTween.Sequence();
            if (delay > 0)
            {
                seq.AppendInterval(delay);
            }

            seq.Append(transform.DOScale(new Vector3(0.9f, 1.1f, 1f), 0.07f));
            seq.Append(transform.DOScale(new Vector3(1.1f, 0.9f, 1f), 0.07f));
            seq.Append(transform.DOScale(Vector3.one, 0.07f));
            seq.OnComplete(() => onCompleted?.Invoke());
            _runningTween.Add(seq);
        }

        public void Jiggly(int times = 1)
        {
            transform.localScale = Vector3.one;
            var seq = DOTween.Sequence();
            seq.Append(transform.DOScale(new Vector3(1.1f, 0.9f, 1f), 0.15f));
            seq.Append(transform.DOScale(Vector3.one, 0.15f));
            seq.SetLoops(times, LoopType.Restart);
            _runningTween.Add(seq);
        }

        public bool IsGoldenEgg() => false;

        private static Color GetDisplayColor(ShelfLayerDisplay display)
        {
            return display switch
            {
                ShelfLayerDisplay.Hidden => Color.clear,
                ShelfLayerDisplay.Second => new Color(0.31f, 0.31f, 0.31f, 1f),
                ShelfLayerDisplay.Top => Color.white,
                _ => Color.red
            };
        }
    }
}