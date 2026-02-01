using MADP.Models;
using MADP.Services.Gold.Interfaces;
using MADP.Views;
using UnityEngine;

namespace MADP.Managers
{
    public class GoldUIManager : MonoBehaviour
    {
        [SerializeField] private GoldView _redGoldView;
        [SerializeField] private GoldView _blueGoldView;
        [SerializeField] private GoldView _yellowGoldView;
        [SerializeField] private GoldView _greenGoldView;
        
        private IGoldService _goldService;

        public void Initialize(IGoldService goldService)
        {
            _goldService = goldService;
            _goldService.OnGoldChanged += UpdateUI;
        }
        
        private void UpdateUI(TeamColor team, int amount)
        {
            switch (team)
            {
                case TeamColor.Red: _redGoldView?.SetGold(amount); break;
                case TeamColor.Blue: _blueGoldView?.SetGold(amount); break;
                case TeamColor.Yellow: _yellowGoldView?.SetGold(amount); break;
                case TeamColor.Green: _greenGoldView?.SetGold(amount); break;
                default: break;
            }
        }
        
        private void OnDestroy()
        {
            if (_goldService != null) 
                _goldService.OnGoldChanged -= UpdateUI;
        }
    }
}