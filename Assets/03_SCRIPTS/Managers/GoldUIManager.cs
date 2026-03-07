using System;
using System.Collections.Generic;
using MADP.Models;
using MADP.Services.Gold.Interfaces;
using MADP.Settings;
using MADP.Views;
using MADP.Views.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Managers
{
    public class GoldUIManager : MonoBehaviour
    {
        [SerializeField] private List<GoldView> goldViews;
        [SerializeField] private Button teamButton;
        [SerializeField] private PlayerInventoryView playerInventoryView;
        
        private IGoldService _goldService;
        
        private Dictionary<TeamColor, GoldView> _activeViews = new ();
        private TeamColorDatabaseSO _teamColorDB;
        private List<LobbySlotModel> _activePlayers;
        private LobbySlotModel _localHumanPlayer;
        
        private bool isVisiable = false;
        private int _currentSelectedSlotIndex = -1;

        private void Awake()
        {
            if (teamButton != null)
            {
                teamButton.onClick.RemoveAllListeners();
                teamButton.onClick.AddListener(SwitchActiveGoldViewsVisibility);
            }

            if (playerInventoryView != null) 
                playerInventoryView.Hide();
            
            SwitchActiveGoldViewsVisibility();
        }

        public void Initialize(IGoldService goldService, 
            List<LobbySlotModel> activePlayers, 
            TeamColorDatabaseSO teamColorDB)
        {
            _goldService = goldService;
            _teamColorDB = teamColorDB;
            _activePlayers = activePlayers;
            _localHumanPlayer = _activePlayers.Find(p => p.PlayerType == PlayerType.Human);
            
            _goldService.OnGoldChanged += UpdateUI;
            
            _activeViews.Clear();
            
            foreach (var view in goldViews) 
            {
                view.Hide();
                view.OnClicked -= HandleGoldViewClicked;
            }

            for (int i = 0; i < activePlayers.Count; i++)
            {
                if (i >= goldViews.Count) break; 
                
                TeamColor team = activePlayers[i].TeamColor;
                GoldView view = goldViews[i];
                Color teamColorUI = _teamColorDB.GetTeamColor(team, Priority.Primary);
                
                view.Setup(activePlayers[i].SlotIndex, teamColorUI);
                view.OnClicked += HandleGoldViewClicked;
                
                _activeViews.Add(team, view);
            }
            
            if (playerInventoryView != null)
            {
                playerInventoryView.OnItemEquipRequested -= HandleItemEquipRequest;
                playerInventoryView.OnItemEquipRequested += HandleItemEquipRequest;
            }
        }
        
        private void HandleItemEquipRequest(ItemDataSO item, UnitModel targetUnit)
        {
            if (_currentSelectedSlotIndex == -1 || _localHumanPlayer == null) return;
    
            var inventoryOwner = _activePlayers.Find(p => p.SlotIndex == _currentSelectedSlotIndex);
            if (inventoryOwner == null) return;
            
            if (inventoryOwner.TeamColor != _localHumanPlayer.TeamColor)
            {
                Debug.LogWarning("Không thể sử dụng vật phẩm từ kho đồ của người chơi khác");
                return;
            }
            
            if (targetUnit.TeamOwner != _localHumanPlayer.TeamColor)
            {
                Debug.LogWarning("Không thể trang bị đồ cho Unit của kẻ thù");
                return;
            }
            
            if (targetUnit.Inventory.IsFull)
            {
                Debug.LogWarning("Kho đồ của Unit này đã đầy");
                return;
            }
            
            if (inventoryOwner.Inventory.RemoveItem(item))
            {
                targetUnit.Inventory.AddItem(item);
                Debug.Log($"Đã trang bị {item.ItemName} cho Unit ID: {targetUnit.Id}");
            }
        }
        
        private void HandleGoldViewClicked(int slotIndex)
        {
            if (_activePlayers == null) return;
            
            if (_currentSelectedSlotIndex == slotIndex)
            {
                _currentSelectedSlotIndex = -1;
                if (playerInventoryView != null) 
                    playerInventoryView.Hide();
                return;
            }
            
            _currentSelectedSlotIndex = slotIndex;
            var player = _activePlayers.Find(p => p.SlotIndex == slotIndex);
            
            if (player != null && playerInventoryView != null)
            {
                bool isLocalPlayer = (_localHumanPlayer != null && player.TeamColor == _localHumanPlayer.TeamColor);
                playerInventoryView.Setup(player.Inventory, isLocalPlayer);
            }
        }

        private void SwitchActiveGoldViewsVisibility()
        {
            isVisiable = !isVisiable;
            if(_activeViews.Values.Count <= 0) return;
            foreach (var view in _activeViews.Values)
            {
                view.gameObject.SetActive(isVisiable);
            }
        }
        
        private void UpdateUI(TeamColor team, int amount)
        {
            if (_activeViews.TryGetValue(team, out GoldView view))
            {
                view.SetGold(amount);
            }
        }
        
        private void OnDestroy()
        {
            if (_goldService != null) 
                _goldService.OnGoldChanged -= UpdateUI;
            
            foreach (var view in _activeViews.Values)
            {
                view.OnClicked -= HandleGoldViewClicked;
            }
            
            if (playerInventoryView != null)
            {
                playerInventoryView.OnItemEquipRequested -= HandleItemEquipRequest;
            }
        }
    }
}