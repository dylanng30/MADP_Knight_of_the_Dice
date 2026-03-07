using System;
using System.Collections;
using System.Collections.Generic;
using MADP.Models;
using MADP.Services.Gold.Interfaces;
using MADP.Services.Shop;
using MADP.Settings;
using MADP.Views.Shop;
using UnityEngine;
using UnityEngine.UI;

namespace MADP.Controllers
{
    public class ShoppingPhaseController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ShopView phaseShopView;
        [SerializeField] private ShopDatabaseSO shopDB;

        [SerializeField] private Transform shopPanel;
        [SerializeField] private Button toggleButton;
        
        private ShopService _shopService;
        private IGoldService _goldService;

        private float _timePerTurn;
        private List<LobbySlotModel> _activePlayers;
        private LobbySlotModel _localPlayer;
        private bool _isOpened;
        
        private Action _cachedCompletionCallback;

        private void Awake()
        {
            _isOpened = false;
            toggleButton.onClick.AddListener(ToggleShopPanel);
        }

        public void Initialize(ShopService shopService, IGoldService goldService, List<LobbySlotModel> activePlayers)
        {
            _shopService = shopService;
            _goldService = goldService;
            _activePlayers = activePlayers;
            
            _localPlayer = _activePlayers.Find(p => p.PlayerType == PlayerType.Human);

            phaseShopView.OnItemPurchaseRequested += HandleLocalPlayerPurchase;
        }

        private void ToggleShopPanel()
        {
            _isOpened = !_isOpened;
            shopPanel.gameObject.SetActive(_isOpened);
        }

        public void SetTimePerTurn(float timePerTurn)
        {
            _timePerTurn = timePerTurn;   
        }

        public void ShowPhaseShop(Action onPhaseCompleted)
        {
            foreach (var player in _activePlayers)
            {
                var generatedItems = _shopService.GenerateRandomShop(shopDB);

                if (player.PlayerType == PlayerType.Human)
                {
                    _isOpened = true;
                    phaseShopView.Setup(generatedItems);
                    phaseShopView.Show();
                }
                else if (player.PlayerType == PlayerType.Bot)
                {
                    _shopService.ProcessBotShopping(player, generatedItems);
                }
            }
            
            StartCoroutine(StartPhaseShopTimer(onPhaseCompleted));
        }
        
        private void HandleLocalPlayerPurchase(ItemDataSO item)
        {
            if (_localPlayer == null) return;
            
            bool success = _shopService.TryBuyItem(item, _localPlayer.Inventory);
            if (success)
            {
                phaseShopView.MarkItemAsPurchased(item);
                int updatedGold = _goldService.GetGold(_localPlayer.TeamColor);
                phaseShopView.RefreshAffordability(updatedGold);
                
                if (AudioController.Instance != null)
                {
                    AudioController.Instance.PlaySound(SoundKey.SFX_BuySuccess);    
                }
            }
            else
            {
                Debug.LogWarning("Không đủ tiền hoặc kho đồ đã đầy!");
            }
        }

        private IEnumerator StartPhaseShopTimer(Action onPhaseCompleted)
        {
            _cachedCompletionCallback = onPhaseCompleted;
            yield return new WaitForSeconds(_timePerTurn);
            
            ClosePhase();
        }

        private void ClosePhase()
        {
            _isOpened = false;
            phaseShopView.Hide();
            _cachedCompletionCallback?.Invoke();
            _cachedCompletionCallback = null;
        }
    }
}