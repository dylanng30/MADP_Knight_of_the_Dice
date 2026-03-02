using System;
using DG.Tweening;
using MADP.Models;
using MADP.Settings;
using MADP.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MADP.Views.Lobby
{
    public class LobbySlotView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("---SET UP---")]
        [SerializeField] private TeamColorDatabaseSO teamColorDB;
        [SerializeField] private RectTransform emptySlot;
        [SerializeField] private RectTransform playerSlot;
        
        [Space(10)]
        [Header("---UI---")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI roleTxt;
        
        [SerializeField] private Transform hostTxt;
        
        [SerializeField] private Image avatarImage;
        [SerializeField] private Image teamImage;
        
        [SerializeField] private Button removeButton;
        [SerializeField] private Button changeColorButton;
        [SerializeField] private Button changeRoleButton;
        
        private RectTransform _rectTransform;
        private Vector2 _originalPosition;
        private Sequence sequence;
        
        private int _slotIndex;
        private Action<int> _onSlotChanged;
        private Action<int> _onColorEdit;
        private Action<int> _onRoleEdit;
        
        private bool _hasPlayer;
        private bool _isHost;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _originalPosition = _rectTransform.anchoredPosition;
        }

        private void OnEnable()
        {
            SlideIn();
        }

        public void Initialize(int index, Action<int> onSlotChanged, Action<int> onColorEdit, Action<int> onRoleEdit)
        {
            _slotIndex = index;
            _onSlotChanged = onSlotChanged;
            _onColorEdit = onColorEdit;
            _onRoleEdit = onRoleEdit;

            changeColorButton.onClick.AddListener(() => _onColorEdit?.Invoke(_slotIndex));
            changeRoleButton.onClick.AddListener(() => _onRoleEdit?.Invoke(_slotIndex));
            removeButton.onClick.AddListener(() => _onSlotChanged?.Invoke(_slotIndex));
        }

        public void Setup(LobbySlotModel model)
        {
            _hasPlayer = model.HasPlayer;
            _isHost = model.IsHost;

            emptySlot.gameObject.SetActive(!_hasPlayer);
            playerSlot.gameObject.SetActive(_hasPlayer);
            hostTxt.gameObject.SetActive(_isHost);
            removeButton.gameObject.SetActive(false);

            if (_hasPlayer)
            {
                SlideIn();
                nameText.text = model.PlayerName;
                teamImage.color = GetTeamColor(model.TeamColor);
                roleTxt.text = model.RoleType.ToString();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_hasPlayer) _onSlotChanged?.Invoke(_slotIndex);
        }
    
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_hasPlayer && !_isHost) removeButton.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_hasPlayer && !_isHost) removeButton.gameObject.SetActive(false);
        }
        
        private Color GetTeamColor(TeamColor color)
        {
            return teamColorDB.GetTeamColor(color, Priority.Primary);
        }

        #region ---ANIMATION ---

        private void SlideIn()
        {
            sequence?.Kill();
            if (_rectTransform != null)
            {
                _rectTransform.anchoredPosition = _originalPosition;
                sequence = UIAnimator.SlideInFromTop(_rectTransform, _rectTransform.rect.height, 1f);
            }
        }

        #endregion
    }
}