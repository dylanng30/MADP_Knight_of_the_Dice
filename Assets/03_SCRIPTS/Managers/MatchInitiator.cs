using System.Collections.Generic;
using System.Linq;
using MADP.Controllers;
using MADP.Models;
using MADP.Services.CellEvent;
using MADP.Services.CellEvent.Interfaces;
using MADP.Services.Combat;
using MADP.Services.Combat.Interfaces;
using MADP.Services.Gold;
using MADP.Services.Gold.Interfaces;
using MADP.Services.Pathfinding;
using MADP.Services.Pathfinding.Interfaces;
using MADP.Services.Shop;
using MADP.Services.VFX;
using MADP.Services.VFX.Interfaces;
using MADP.Settings;
using MADP.Utilities;
using MADP.Views;
using UnityEngine;

namespace MADP.Managers
{
    public class MatchInitiator : MonoBehaviour
    {
        [SerializeField] private GoldUIManager _goldUIManager;
        [SerializeField] private DiceView diceView;
        
        [Header("---CONTROLLERS---")]
        [SerializeField] private BoardController _boardController;
        [SerializeField] private TurnController _turnController;
        [SerializeField] private UnitDeckController _unitDeckController;
        [SerializeField] private ShoppingPhaseController _shoppingController;
        
        [Header("---COLOR DB---")]
        [SerializeField] private TeamColorDatabaseSO teamColorDB;
        
        private IGoldService _goldService;
        private IPathfindingService _pathfindingService;
        private ICombatService _combatService;
        private ICellEventService _cellEventService;
        private IVFXService _vfxService;
        private ShopService _shopService;
        
        private void Awake()
        {
            if (SceneController.Instance != null)
                SceneController.Instance.OnLoadingFinished += StartMatch;
        }
        
        private void Start()
        {
            if (SceneController.Instance == null)
                StartMatch();
        }

        private void OnDestroy()
        {
            if (SceneController.Instance != null)
            {
                SceneController.Instance.OnLoadingFinished -= StartMatch;
            }
                
            if (AudioController.Instance != null)
            {
                AudioController.Instance.DisconnectFromMatch();
            }
        }

        private void StartMatch()
        {
            MatchSettingsModel settings = GameManager.Instance != null 
                ? GameManager.Instance.CurrentMatchSettings 
                : GetMockSettings();
            
            List<LobbySlotModel> activePlayers = settings.Slots
                .Where(slot => slot.PlayerType != PlayerType.Empty)
                .ToList();
            
            var localPlayerSlot = activePlayers.FirstOrDefault(x => x.PlayerType == PlayerType.Human);
            
            //Color
            teamColorDB = teamColorDB != null ? teamColorDB : Resources.Load<TeamColorDatabaseSO>("TeamColorDB");
            diceView.Setup(teamColorDB.GetMapColor(settings.SelectedMap, Priority.Tertiary));
            
            //VFX
            VFXDatabaseSO vfxDB = Resources.Load<VFXDatabaseSO>("VFXDatabase");
            _vfxService = new VFXService(vfxDB);
            GameObject vfxContainer = new GameObject("VFX_Pool_Container"); 
            
            //SERVICES
            _goldService = new GoldService();
            _pathfindingService = new PathfindingService();
            _combatService = new CombatService();
            _cellEventService = new CellEventService(_goldService, _vfxService);
            _shopService = new ShopService(_goldService);
            
            //CONTROLLERS
            
            if (AudioController.Instance != null)
            {
                AudioController.Instance.ConnectToMatch(_goldService);
            }
            
            _shoppingController.Initialize(_shopService, _goldService, activePlayers);
            _shoppingController.SetTimePerTurn(settings.TimePerTurn);
            
            _boardController.Initialize(
                _goldService, 
                _pathfindingService, 
                _combatService, 
                _cellEventService, 
                _vfxService,
                settings.RedCellCount,
                settings.YellowCellCount,
                settings.PurpleCellCount,
                settings.GreenCellCount,
                activePlayers, 
                settings.SelectedMap, 
                teamColorDB);
            _turnController.Initialize(_shoppingController, _goldService, activePlayers, diceView, settings.TimePerTurn);
            _goldUIManager.Initialize(_goldService, activePlayers, teamColorDB);
            _vfxService.Initialize(vfxContainer.transform);
            _goldService.Initialize(Constants.InitialGold, activePlayers);
            
            
            if (localPlayerSlot != null && _unitDeckController != null)
            {
                List<UnitModel> myUnits = _boardController.GetAllUnitsByColor(localPlayerSlot.TeamColor);
                _unitDeckController.Initialize(
                    _turnController, 
                    _boardController, 
                    _goldService, 
                    myUnits);
            }
        }
        
        private MatchSettingsModel GetMockSettings()
        {
            return new MatchSettingsModel
            {
                Slots = new[]
                {
                    new LobbySlotModel(0, TeamColor.Red) { PlayerType = PlayerType.Human },
                    new LobbySlotModel(1, TeamColor.Blue) { PlayerType = PlayerType.Bot }
                }
            };
        }
    }
}