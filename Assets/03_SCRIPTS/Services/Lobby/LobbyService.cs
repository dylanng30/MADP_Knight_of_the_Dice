using System;
using System.Collections.Generic;
using System.Linq;
using MADP.Models;
using MADP.Settings;
using MADP.Utilities;
using UnityEngine;

namespace MADP.Services.Lobby
{
    public class LobbyService
    {
        private LobbyModel _lobbyModel;
        private MatchSettingsModel _currentMatchSettings;

        public LobbyModel GetLobby() => _lobbyModel;
        public LobbySlotModel[] GetSlots() => _lobbyModel?.Slots;
        public MatchSettingsModel GetMatchSettings() => _currentMatchSettings;

        public void InitializeLobby(int roomId)
        {
            _lobbyModel = new LobbyModel
            {
                RoomId = roomId,
                Slots = new LobbySlotModel[4]
            };

            _lobbyModel.Slots[0] = new LobbySlotModel(0, TeamColor.Red)
            {
                PlayerType = PlayerType.Bot,
                IsHost = true
            };
            _lobbyModel.Slots[1] = new LobbySlotModel(1, TeamColor.None);
            _lobbyModel.Slots[2] = new LobbySlotModel(2, TeamColor.None);
            _lobbyModel.Slots[3] = new LobbySlotModel(3, TeamColor.None);
            
            _currentMatchSettings = new MatchSettingsModel
            {
                TimePerTurn = 30,
                SelectedMap = MapType.Desert,
                RedCellCount = 1,
                YellowCellCount = 1,
                PurpleCellCount = 1,
                Slots = _lobbyModel.Slots
            };
        }
        
        public void SetAllBotsDifficulty(BotDifficulty difficulty)
        {
            if (_lobbyModel == null || _lobbyModel.Slots == null) return;
    
            foreach (var slot in _lobbyModel.Slots)
            {
                slot.BotType = difficulty;
            }
        }

        public void UpdateMatchSettings(int time, MapType map, int redCellCount, int yellowCellCount, int purpleCellCount, int healCells)
        {
            _currentMatchSettings.TimePerTurn = time;
            _currentMatchSettings.SelectedMap = map;
            _currentMatchSettings.RedCellCount = redCellCount;
            _currentMatchSettings.YellowCellCount = yellowCellCount;
            _currentMatchSettings.PurpleCellCount = purpleCellCount;
            _currentMatchSettings.GreenCellCount = healCells;
        }

        public void ToggleSlotState(int index)
        {
            if (index <= 0 || index >= _lobbyModel.Slots.Length) return;

            var slot = _lobbyModel.Slots[index];

            if (slot.PlayerType == PlayerType.Empty)
            {
                slot.PlayerType = PlayerType.Bot;
                slot.PlayerName = "Bot " + (index + 1);
                slot.TeamColor = GetFirstAvailableColor();
            }
            else
            {
                slot.PlayerType = PlayerType.Empty;
                slot.PlayerName = "Empty";
            }
        }

        public void SetSlotColor(int index, TeamColor newColor)
        {
            if (index >= 0 && index < _lobbyModel.Slots.Length)
                _lobbyModel.Slots[index].TeamColor = newColor;
        }

        public void SetSlotRole(int index, RoleType newRole)
        {
            if (index >= 0 && index < _lobbyModel.Slots.Length)
                _lobbyModel.Slots[index].RoleType = newRole;
        }

        public List<TeamColor> GetTakenColors(int excludeSlotIndex)
        {
            return _lobbyModel.Slots
                .Where(s => s.SlotIndex != excludeSlotIndex && s.HasPlayer)
                .Select(s => s.TeamColor)
                .ToList();
        }

        private TeamColor GetFirstAvailableColor()
        {
            List<TeamColor> takenColors = _lobbyModel.Slots
                .Where(s => s.HasPlayer && s.TeamColor != TeamColor.None)
                .Select(s => s.TeamColor)
                .ToList();

            foreach (TeamColor color in Enum.GetValues(typeof(TeamColor)))
            {
                if (color != TeamColor.None && !takenColors.Contains(color))
                {
                    return color;
                }
            }

            return TeamColor.None;
        }

        public MatchSettingsModel GetFinalizedMatchSettings()
        {
            List<LobbySlotModel> finalizedSlots = new List<LobbySlotModel>();

            foreach (var originalSlot in _lobbyModel.Slots)
            {
                LobbySlotModel newSlot = new LobbySlotModel(originalSlot.SlotIndex, originalSlot.TeamColor);
                newSlot.PlayerType = originalSlot.PlayerType;

                if (originalSlot.RoleType == RoleType.Random)
                {
                    newSlot.RoleType = EnumUtils.GetRandomRoleConcrete();
                    //Debug.Log($"Slot {newSlot.SlotIndex} Role 'Random' -> Resolved to '{newSlot.RoleType}'");
                }
                else
                {
                    newSlot.RoleType = originalSlot.RoleType;
                }

                finalizedSlots.Add(newSlot);
            }
            _currentMatchSettings.Slots = finalizedSlots.ToArray();
            return _currentMatchSettings;

            /*return new MatchSettingsModel
            {
                Slots = finalizedSlots.ToArray(),
                SelectedMap = _currentMatchSettings.SelectedMap,
            };*/

        }
    }
}