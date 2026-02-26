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
        [SerializeField] private List<GoldView> goldViews;
        
        private IGoldService _goldService;
        private Dictionary<TeamColor, GoldView> _activeViews = new ();
        private TeamColorDatabaseSO _teamColorDB;

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