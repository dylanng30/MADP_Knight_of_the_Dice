using System;
using System.Collections;
using System.Collections.Generic;
using MADP.Models;
using MADP.Models.Inventory;
using MADP.Services.AI;
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
        [Header("References")] [SerializeField]
        private ShopView phaseShopView;

        [SerializeField] private ShopDatabaseSO shopDB;

        [SerializeField] private Transform shopPanel;
        [SerializeField] private Button toggleButton;

        private ShopService _shopService;
        private IGoldService _goldService;
        private BotDecisionService _botDecisionService;

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

        public void Initialize(
            ShopService shopService,
            IGoldService goldService,
            BotDecisionService botDecisionService,
            List<LobbySlotModel> activePlayers)
        {
            _shopService = shopService;
            _goldService = goldService;
            _botDecisionService = botDecisionService;
            _activePlayers = activePlayers;

            _localPlayer = _activePlayers.Find(p => p.PlayerType == PlayerType.Human);

            phaseShopView.OnItemPurchaseRequested += HandleLocalPlayerPurchase;
            _agentShops.Clear();
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
            _agentShops.Clear();
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
                    int currentGold = _goldService.GetGold(player.TeamColor);
                    int availableSlots = PlayerInventoryModel.ItemSlots - player.Inventory.Items.Count;

                    List<ItemDataSO> itemsToBuy = _botDecisionService.GetShoppingList(player.TeamColor, currentGold,
                        availableSlots, generatedItems);

                    foreach (var item in itemsToBuy)
                    {
                        _shopService.TryBuyItem(item, player.Inventory);
                        Debug.Log($"Bot {player.TeamColor} đã quyết định mua: {item.ItemName}");
                    }
                }
                else if (player.PlayerType == PlayerType.MLAgent)
                {
                    // Store items for the agent to observe
                    _agentShops[player.TeamColor] = generatedItems;

                    // Trigger agent turn
                    var turnCtrl = GetComponentInParent<TurnController>();
                    if (turnCtrl != null)
                    {
                        turnCtrl.OnMLAgentShoppingTurn?.Invoke(player.TeamColor, generatedItems);
                    }
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

        #region ML-Agent Integration

        private Dictionary<TeamColor, List<ItemDataSO>> _agentShops = new();

        public bool TryBuyAgentItem(TeamColor team, int itemIndex)
        {
            if (!_agentShops.TryGetValue(team, out var items) || itemIndex >= items.Count) return false;

            var player = _activePlayers.Find(p => p.TeamColor == team);
            if (player == null) return false;

            var item = items[itemIndex];
            if (item == null) return false; // Item already bought or empty

            bool success = _shopService.TryBuyItem(item, player.Inventory);

            if (success)
            {
                items[itemIndex] = null;
                Debug.Log($"[MLAgent] Team {team} đã mua {item.ItemName} qua ML-Agent Action");
            }

            return success;
        }

        #endregion

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