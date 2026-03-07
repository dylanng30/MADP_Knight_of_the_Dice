using System;
using MADP.Settings;
using MADP.Views.Unit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MADP.Views.Inventory
{
    [RequireComponent(typeof(CanvasGroup))]
    public class InventorySlotView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image itemIcon;

        private ItemDataSO _currentItem;
        private bool _isDraggable = false;
        
        private GameObject _dragPreview; 
        private CanvasGroup _canvasGroup;
        
        public Action<ItemDataSO, UnitView> OnItemDroppedOnUnit;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        public void Setup(ItemDataSO item, bool isDraggable = false)
        {
            _currentItem = item;
            _isDraggable = isDraggable;
            
            if (item != null)
            {
                itemIcon.sprite = item.Icon;
            }
            
            gameObject.SetActive(item != null);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_currentItem == null || !_isDraggable) return;
            
            Canvas mainCanvas = GetComponentInParent<Canvas>();
            
            _dragPreview = Instantiate(gameObject, mainCanvas.transform);
            
            _dragPreview.transform.SetAsLastSibling();
            
            Destroy(_dragPreview.GetComponent<InventorySlotView>());
    
            CanvasGroup previewGroup = _dragPreview.GetComponent<CanvasGroup>();
            if (previewGroup == null) previewGroup = _dragPreview.AddComponent<CanvasGroup>();
    
            previewGroup.blocksRaycasts = false;
            previewGroup.alpha = 0.8f;
            
            if (_canvasGroup != null) 
                _canvasGroup.alpha = 0.5f; 
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_currentItem == null || !_isDraggable || _dragPreview == null) return;
            
            _dragPreview.transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_currentItem == null || !_isDraggable) return;
            
            if (_dragPreview != null)
            {
                Destroy(_dragPreview);
            }
            
            if (_canvasGroup != null)
                _canvasGroup.alpha = 1f;
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                UnitView unitView = hit.collider.GetComponentInParent<UnitView>();
                if (unitView != null)
                {
                    OnItemDroppedOnUnit?.Invoke(_currentItem, unitView);
                }
            }
        }
    }
}