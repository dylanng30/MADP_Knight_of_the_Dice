using System;
using System.Collections.Generic;
using MADP.Models;
using MADP.Services.Gold.Interfaces;
using MADP.Settings;
using MADP.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Managers
{
    public class GoldUIManager : MonoBehaviour
    {
        [SerializeField] private List<GoldView> goldViews;
        [SerializeField] private Button teamButton;
        
        private IGoldService _goldService;
        private Dictionary<TeamColor, GoldView> _activeViews = new ();
        private TeamColorDatabaseSO _teamColorDB;
        
        private bool isVisiable = false;

        private void Awake()
        {
            if (teamButton != null)
            {
                teamButton.onClick.RemoveAllListeners();
                teamButton.onClick.AddListener(SwitchActiveGoldViewsVisibility);
            }
        }

        public void Initialize(IGoldService goldService, 
            List<LobbySlotModel> activePlayers, 
            TeamColorDatabaseSO teamColorDB)
        {
            _goldService = goldService;
            _teamColorDB = teamColorDB;
            _goldService.OnGoldChanged += UpdateUI;
            
            _activeViews.Clear();
            
            foreach (var view in goldViews) view.Hide();

            for (int i = 0; i < activePlayers.Count; i++)
            {
                if (i >= goldViews.Count) break; 

                TeamColor team = activePlayers[i].TeamColor;
                GoldView view = goldViews[i];
                Color teamColorUI = _teamColorDB.GetTeamColor(team, Priority.Primary);
                Color frameColorUI = _teamColorDB.GetTeamColor(team, Priority.Tertiary);
                view.Setup(i, teamColorUI, frameColorUI, activePlayers[i].AvatarPath);
                _activeViews.Add(team, view);
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
        }

    }
}