using System.Collections.Generic;
using System.Linq;
using MADP.Managers;
using MADP.Models;
using MADP.Services.Lobby;
using MADP.Settings;
using MADP.Views.Lobby;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MADP.Controllers
{
    public class LobbyController : MonoBehaviour
    {
        [SerializeField] private LobbyView lobbyView;
        [SerializeField] private ColorSettingView colorSettingView;
        [SerializeField] private RoleSettingView roleSettingView;
        [SerializeField] private MatchSettingsView matchSettingsView;
        
        [Header("GameSettings")]
        [SerializeField] private GameSettingsController gameSettingsController;
        [SerializeField] private Button gameSettingsButton;

        private LobbyService _lobbyService = new();

        private void Start()
        {
            if (gameSettingsController != null)
            {
                gameSettingsController.Initialize();
                gameSettingsButton.onClick.AddListener(gameSettingsController.OpenSettings);
            }
            
            
            lobbyView.OnSlotActionRequested += HandleSlotAction;
            lobbyView.OnColorEditRequested += HandleColorEditRequested;
            lobbyView.OnRoleEditRequested += HandleRoleEditRequested;
            lobbyView.OnStartGameClicked += HandleStartGame;
            lobbyView.OnMatchSettingsClicked += HandleMatchSettingRequested;

            colorSettingView.OnColorSaved += HandleColorSaveRequested;
            roleSettingView.OnRoleSaved += HandleRoleSaveRequested;
            matchSettingsView.OnSettingsSaved += HandleMatchSettingSaved;
            CreateLobby(1);
            
        }
        private void OnDestroy()
        {
            // Hủy đăng ký khi Controller bị hủy
            if (lobbyView != null)
            {
                lobbyView.OnSlotActionRequested -= HandleSlotAction;
                lobbyView.OnColorEditRequested -= HandleColorEditRequested;
                lobbyView.OnRoleEditRequested -= HandleRoleEditRequested;
                lobbyView.OnStartGameClicked -= HandleStartGame;
            }
            if (colorSettingView != null) colorSettingView.OnColorSaved -= HandleColorSaveRequested;
            if (roleSettingView != null) roleSettingView.OnRoleSaved -= HandleRoleSaveRequested;
        }

        public void CreateLobby(int roomId)
        {
            _lobbyService.InitializeLobby(roomId);
            var lobbyModel = _lobbyService.GetLobby();
            
            lobbyView.SetRoomID(roomId);
            lobbyView.Initialize(lobbyModel.Slots);
            
            roleSettingView.Hide();
            colorSettingView.Hide();
            matchSettingsView.Hide();
        }

        #region ---HANDLERS---
        private void HandleSlotAction(int slotIndex)
        {
            _lobbyService.ToggleSlotState(slotIndex);
            var updatedModel = _lobbyService.GetSlots()[slotIndex];
            lobbyView.RefreshSlot(slotIndex, updatedModel);
        }
        
        //COLOR
        private void HandleColorEditRequested(int slotIndex)
        {
            var currentSlot = _lobbyService.GetSlots()[slotIndex];
            var takenColors = _lobbyService.GetTakenColors(slotIndex);
            //colorSettingView.Show(currentSlot, _lobbyService.GetSlots());
            colorSettingView.Show(currentSlot.TeamColor, takenColors, slotIndex);
        }
        private void HandleColorSaveRequested(int slotIndex, TeamColor newColor)
        {
            _lobbyService.SetSlotColor(slotIndex, newColor);
            var updatedModel = _lobbyService.GetSlots()[slotIndex];
            lobbyView.RefreshSlot(slotIndex, updatedModel);
            colorSettingView.Hide();
        }
        
        //ROLE
        private void HandleRoleEditRequested(int slotIndex)
        {
            var currentSlot = _lobbyService.GetSlots()[slotIndex];
            roleSettingView.Show(currentSlot.RoleType, slotIndex);
        }
        private void HandleRoleSaveRequested(int slotIndex, RoleType newRoleType)
        {
            _lobbyService.SetSlotRole(slotIndex, newRoleType);
            lobbyView.RefreshSlot(slotIndex, _lobbyService.GetSlots()[slotIndex]);
            roleSettingView.Hide();
        }
        //Match settings
        private void HandleMatchSettingRequested()
        {
            MatchSettingsModel currentSettings = _lobbyService.GetMatchSettings();
            matchSettingsView.Show(currentSettings);
        }

        private void HandleMatchSettingSaved(int time, MapType map, int redCells, int yellowCells, int purpleCells)
        {
            _lobbyService.UpdateMatchSettings(time, map, redCells, yellowCells, purpleCells);
        }
        private void HandleStartGame()
        {
            Debug.Log("Game Started!");
            var matchSettings = _lobbyService.GetFinalizedMatchSettings();
            int activePlayers = 0;
            foreach (var slot in matchSettings.Slots)
                if (slot.PlayerType != PlayerType.Empty) activePlayers++;

            if (activePlayers < 2)
            {
                Debug.LogWarning("Cần ít nhất 2 người chơi để bắt đầu!");
                return;
            }

            GameManager.Instance.CurrentMatchSettings = matchSettings;          
            SceneManager.LoadScene("Match");
        }
        #endregion

        
    }
}