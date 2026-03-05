using System.Collections.Generic;
using MADP.Models;
using MADP.Services.Gold.Interfaces;
using MADP.Settings;
using MADP.Views;
using UnityEngine;

namespace MADP.Controllers
{
    public class UnitDeckController : MonoBehaviour
    {
        [SerializeField] private UnitAvatarDatabaseSO unitAvatarDB;
        [SerializeField] private UnitCardView cardPrefab;
        [SerializeField] private Transform cardContainer;
        [SerializeField] private GameObject CardViews;
        
        private TurnController _turnController;
        private BoardController _boardController;
        
        private IGoldService _goldService;
        
        private List<UnitCardView> _spawnedCards = new ();
        private List<UnitModel> _currentUnits = new ();
        private bool _isInitialized = false;
        
        public void Initialize(
            TurnController turnController, 
            BoardController boardController, 
            IGoldService goldService, 
            List<UnitModel> playerUnits)
        {
            _turnController = turnController;
            _boardController = boardController;
            _goldService = goldService;
            _currentUnits = playerUnits;
            
            ShowViews(false);
            
            GenerateCards();
            
            _goldService.OnGoldChanged += HandleGoldChanged;
            turnController.OnDiceRolled += HandleDiceRolled;
            
            _isInitialized = true;
        }

        private void OnDestroy()
        {
            if (_isInitialized)
            {
                if (_goldService != null) _goldService.OnGoldChanged -= HandleGoldChanged;
                if (_turnController != null)
                {
                    _turnController.OnDiceRolled -= HandleDiceRolled;
                }
            }
        }

        public void ShowViews(bool isShown)
        {
            CardViews.SetActive(isShown);
        }

        private void GenerateCards()
        {
            foreach (Transform child in cardContainer) Destroy(child.gameObject);
            _spawnedCards.Clear();

            foreach (var unit in _currentUnits)
            {
                UnitCardView card = Instantiate(cardPrefab, cardContainer);
                Sprite avatar = unitAvatarDB.GetAvatar(unit.TeamOwner, unit.Id);
                card.Setup(unit, avatar, OnCardClicked);
                _spawnedCards.Add(card);
            }
        }
        
        private void HandleDiceRolled(int diceValue)
        {
            if (_turnController.IsPlayerTurn)
            {
                RefreshCardStates();
            }
            
            ShowViews(_turnController.IsPlayerTurn && diceValue == 6);
        }
        private void HandleGoldChanged(TeamColor team, int amount)
        {
            if (_turnController.IsPlayerTurn && team == _turnController.CurrentTeam)
            {
                RefreshCardStates();
            }
        }

        private void RefreshCardStates()
        {
            int currentDice = _turnController.CurrentDiceValue;
            TeamColor myTeam = _turnController.CurrentTeam;
            int myGold = _goldService.GetGold(myTeam);

            for (int i = 0; i < _currentUnits.Count; i++)
            {
                UnitModel unit = _currentUnits[i];
                UnitCardView card = _spawnedCards[i];
                
                bool canSpawn = _boardController.CanSpawnUnit(unit, currentDice);

                card.SetInteractable(canSpawn, myGold);
            }
        }

        private void OnCardClicked(UnitModel unit)
        {
            if (_turnController.IsPlayerTurn)
            {
                _turnController.HandleUnitClicked(unit);
                ShowViews(false);
            }
        }
    }
}