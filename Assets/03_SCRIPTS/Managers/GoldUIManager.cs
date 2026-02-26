using System.Collections.Generic;
using MADP.Models;
using MADP.Services.Gold.Interfaces;
using MADP.Settings;
using MADP.Views;
using TMPro;
using UnityEngine;

namespace MADP.Managers
{
    public class GoldUIManager : MonoBehaviour
    {
        [SerializeField] private TeamColorDatabaseSO teamColorDB;
        [SerializeField] private List<GoldView> goldViews;
        
        private IGoldService _goldService;
        private Dictionary<TeamColor, GoldView> _activeViews = new ();

        public void Initialize(IGoldService goldService, List<LobbySlotModel> activePlayers)
        {
            _goldService = goldService;
            _goldService.OnGoldChanged += UpdateUI;
            
            _activeViews.Clear();
            
            foreach (var view in goldViews) view.Hide();

            for (int i = 0; i < activePlayers.Count; i++)
            {
                if (i >= goldViews.Count) break; 

                TeamColor team = activePlayers[i].TeamColor;
                GoldView view = goldViews[i];
                Color teamColorUI = teamColorDB.GetColor(team, Priority.Primary);
                Color frameColorUI = teamColorDB.GetColor(team, Priority.Tertiary);
                view.Setup(team, teamColorUI, frameColorUI, activePlayers[i].AvatarPath);
                _activeViews.Add(team, view);
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