using System;
using System.Collections.Generic;
using MADP.Models;
using MADP.Settings;
using UnityEngine;

namespace MADP.Services
{
    public class UnitModelGenerationService
    {
        private readonly TeamStatDatabaseSO _teamStatDB;
        public UnitModelGenerationService(TeamStatDatabaseSO teamStatDB) 
        { 
            _teamStatDB = teamStatDB;
        }
        public Dictionary<TeamColor, List<UnitModel>> CreateAllUnits(List<LobbySlotModel> activePlayers)
        {
            var allUnits = new Dictionary<TeamColor, List<UnitModel>>();
            
            foreach (var player in activePlayers)
            {
                allUnits.Add(player.TeamColor, CreateTeamUnit(player.TeamColor, player.RoleType));
            }   
            
            return allUnits;
        }

        private List<UnitModel> CreateTeamUnit(TeamColor teamColor, RoleType roleType)
        {
            List<UnitModel> teamUnits = new List<UnitModel>();
            for(int i = 0; i < 4; i++)
            {
                UnitStatModel statModel = GetStatModel(i, roleType);
                UnitModel model = new UnitModel(i, teamColor, statModel, roleType);
                teamUnits.Add(model);
            }
            
            return teamUnits;
        }

        private UnitStatModel GetStatModel(int level, RoleType roleType)
        {
            var teamStat = _teamStatDB.TeamStats.Find(x => x.Role == roleType);

            if (teamStat.UnitStats == null || teamStat.UnitStats.Count == 0)
            {
                Debug.LogWarning($"Thiếu chỉ số cho Role {roleType}, dùng mặc định.");
                return new UnitStatModel(10, 1, 0, roleType);
            }

            var unitStat = teamStat.UnitStats.Find(x => x.Level == level);
            return new UnitStatModel(unitStat.MaxHealth, unitStat.Damage, unitStat.Armor, roleType);
        }
    }
}