using System;
using System.Collections.Generic;
using MADP.Models;

namespace MADP.Services
{
    public class UnitModelGenerationService
    {
        public Dictionary<TeamColor, List<UnitModel>> CreateAllUnits()
        {
            var allUnits = new Dictionary<TeamColor, List<UnitModel>>();
            
            allUnits.Add(TeamColor.Red, CreateTeamUnit(TeamColor.Red));
            allUnits.Add(TeamColor.Blue, CreateTeamUnit(TeamColor.Blue));
            allUnits.Add(TeamColor.Yellow, CreateTeamUnit(TeamColor.Yellow));
            allUnits.Add(TeamColor.Green, CreateTeamUnit(TeamColor.Green));
            
            return allUnits;
        }

        private List<UnitModel> CreateTeamUnit(TeamColor teamColor)
        {
            List<UnitModel> teamUnits = new List<UnitModel>();
            for(int i = 0; i < 4; i++)
            {
                UnitStatModel statModel = new UnitStatModel(5, 2, 1);
                UnitModel model = new UnitModel(i, teamColor, statModel);
                teamUnits.Add(model);
            }
            
            return teamUnits;
        }
    }
}