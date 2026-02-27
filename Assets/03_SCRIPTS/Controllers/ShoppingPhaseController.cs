using System;
using System.Collections;
using System.Collections.Generic;
using MADP.Models;
using MADP.Services.Shop;
using MADP.Settings;
using MADP.Views.Shop;
using UnityEngine;

namespace MADP.Controllers
{
    public class ShoppingPhaseController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ShopView phaseShopView;
        [SerializeField] private ShopDatabaseSO shopDB;
        [SerializeField] private BoardController boardController;
        
        private ShopService _shopService;

        private float _timePerTurn;
        private Dictionary<LobbySlotModel, List<ItemDataSO>> _shopMapper = new();
        
        public void Initialize(ShopService shopService, List<LobbySlotModel> activePlayers)
        {
            _shopService = shopService;

            _shopMapper.Clear();
            foreach (var player in activePlayers)
            {
                _shopMapper.Add(player, new List<ItemDataSO>());
            }
        }

        public void SetTimePerTurn(float timePerTurn)
        {
            _timePerTurn = timePerTurn;   
        }

        public void ShowPhaseShop(Action onPhaseCompleted)
        {
            foreach (var player in _shopMapper.Keys)
            {
                _shopMapper[player].Clear();
                var newItems = _shopService.GenerateRandomShop(shopDB);
                _shopMapper[player].AddRange(newItems);

                if (player.IsHost)
                {
                    phaseShopView.Setup(newItems);
                    phaseShopView.Show();
                }
            }
            StartCoroutine(StartPhaseShop(onPhaseCompleted));
        }

        private IEnumerator StartPhaseShop(Action onPhaseCompleted)
        {
            yield return new WaitForSeconds(_timePerTurn);
            phaseShopView.Hide();
            onPhaseCompleted?.Invoke();
        }
    }
}