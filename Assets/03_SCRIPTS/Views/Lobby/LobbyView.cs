using System;
using System.Collections.Generic;
using MADP.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Views.Lobby
{
    public class LobbyView : MonoBehaviour
    {
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _homeButton;
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button matchSettingsButton;

        [SerializeField] private TextMeshProUGUI roomIdTxt;

        [SerializeField] private List<LobbySlotView> _slotViews = new ();
        
        public Action<int> OnSlotActionRequested;
        public Action<int> OnColorEditRequested;
        public Action<int> OnRoleEditRequested;
        public Action OnStartGameClicked;
        public Action OnMatchSettingsClicked;
        
        private void Awake()
        {
            _startGameButton.onClick.AddListener(() => OnStartGameClicked?.Invoke());
            matchSettingsButton.onClick.AddListener(() => OnMatchSettingsClicked?.Invoke());
            
            for (int i = 0; i < _slotViews.Count; i++)
            {
                var view = _slotViews[i];
                view.Initialize(i, 
                    (idx) => OnSlotActionRequested?.Invoke(idx),
                    (idx) => OnColorEditRequested?.Invoke(idx),
                    (idx) => OnRoleEditRequested?.Invoke(idx)
                );
            }
        }
        public void Initialize(LobbySlotModel[] slotModels)
        {
            for (int i = 0; i < slotModels.Length && i < _slotViews.Count; i++)
            {
                _slotViews[i].Setup(slotModels[i]);
            }
        }

        public void SetRoomID(int roomID)
        {
            if (roomIdTxt) roomIdTxt.text = $"Room: {roomID}";
        }

        public void RefreshSlot(int index, LobbySlotModel slotModel)
        {
            if(index >= 0 && index < _slotViews.Count)
                _slotViews[index].Setup(slotModel);
        }
    }
}